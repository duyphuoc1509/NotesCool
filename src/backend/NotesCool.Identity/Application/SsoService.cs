using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NotesCool.Identity.Application.Abstractions;
using NotesCool.Identity.Contracts;
using NotesCool.Identity.Infrastructure;

namespace NotesCool.Identity.Application;

public sealed class SsoService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly HttpClient _httpClient;
    private readonly MicrosoftSsoOptions _options;

    public SsoService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator jwtTokenGenerator,
        HttpClient httpClient,
        IOptions<MicrosoftSsoOptions> options)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _httpClient = httpClient;
        _options = options.Value;
    }

    public string GetMicrosoftLoginUrl(string redirectUri)
    {
        var scopes = string.Join(" ", _options.Scopes);
        
        var queryParams = new[]
        {
            $"client_id={_options.ClientId}",
            "response_type=code",
            $"redirect_uri={Uri.EscapeDataString(redirectUri)}",
            "response_mode=query",
            $"scope={Uri.EscapeDataString(scopes)}"
        };

        return $"{_options.AuthorizationEndpoint}?{string.Join("&", queryParams)}";
    }

    public async Task<AuthResponse> HandleMicrosoftCallbackAsync(string code, string redirectUri)
    {
        // 1. Exchange code for access token
        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", _options.ClientId! },
            { "client_secret", _options.ClientSecret! },
            { "code", code },
            { "redirect_uri", redirectUri },
            { "grant_type", "authorization_code" }
        });

        var tokenResponse = await _httpClient.PostAsync(_options.TokenEndpoint, tokenRequest);
        if (!tokenResponse.IsSuccessStatusCode)
        {
            var error = await tokenResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to exchange code for token: {error}");
        }

        using var tokenDoc = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync());
        var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString()!;

        // 2. Fetch user profile from Microsoft Graph
        var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        var profileResponse = await _httpClient.SendAsync(request);
        if (!profileResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Failed to fetch user profile from Microsoft.");
        }

        var profileContent = await profileResponse.Content.ReadAsStringAsync();
        using var profileDoc = JsonDocument.Parse(profileContent);
        
        var email = profileDoc.RootElement.TryGetProperty("userPrincipalName", out var upnProp) ? upnProp.GetString() : null;
        if (string.IsNullOrEmpty(email))
        {
            if (profileDoc.RootElement.TryGetProperty("mail", out var mailProp))
            {
                email = mailProp.GetString();
            }
        }
        
        var displayName = profileDoc.RootElement.TryGetProperty("displayName", out var nameProp) ? nameProp.GetString() : email;
        var subjectId = profileDoc.RootElement.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(subjectId))
        {
            throw new InvalidOperationException("Incomplete profile returned from Microsoft.");
        }

        // 3. Link or create user
        var user = await _userManager.FindByLoginAsync("Microsoft", subjectId);
        
        if (user == null)
        {
            user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    DisplayName = displayName!,
                    Status = AccountStatus.Active,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException("Failed to create user account.");
                }
            }

            var linkResult = await _userManager.AddLoginAsync(user, new UserLoginInfo("Microsoft", subjectId, "Microsoft"));
            if (!linkResult.Succeeded)
            {
                throw new InvalidOperationException("Failed to link Microsoft account to user.");
            }
        }

        if (user.Status != AccountStatus.Active)
        {
            throw new AccountInactiveException(user.Status);
        }

        return _jwtTokenGenerator.CreateToken(user);
    }
}
