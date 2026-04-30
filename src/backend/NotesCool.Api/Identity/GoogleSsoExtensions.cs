using System.Security.Cryptography;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Globalization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NotesCool.Api.Auth;
using NotesCool.Api.Contracts;
using NotesCool.Api.Configuration;
using NotesCool.Identity.Application.Abstractions;
using NotesCool.Identity.Contracts;
using NotesCool.Identity.Infrastructure;
using NotesCool.Shared.Security;

namespace NotesCool.Api.Identity;

public static class GoogleSsoExtensions
{
    public static IEndpointRouteBuilder MapGoogleSsoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/sso/google").WithTags("Google SSO");

        group.MapGet("/login", (HttpContext httpContext, IOptions<SsoOptions> options) =>
        {
            var googleOptions = options.Value.Providers.FirstOrDefault(p => p.Name.Equals("Google", StringComparison.OrdinalIgnoreCase));
            if (googleOptions is null || !googleOptions.Enabled)
            {
                return Results.BadRequest(new SsoErrorResponse("provider_disabled", "Google SSO is not enabled."));
            }

            var redirectUri = GetRedirectUri(googleOptions, httpContext);
            var stateBytes = new byte[32];
            RandomNumberGenerator.Fill(stateBytes);
            var state = Convert.ToBase64String(stateBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');

            var nonceBytes = new byte[32];
            RandomNumberGenerator.Fill(nonceBytes);
            var nonce = Convert.ToBase64String(nonceBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');

            // Store state and nonce in a secure, HttpOnly, same-site cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Should ideally be true in production
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(15)
            };
            
            httpContext.Response.Cookies.Append("sso_state", state, cookieOptions);
            httpContext.Response.Cookies.Append("sso_nonce", nonce, cookieOptions);

            var queryStr = $"?client_id={googleOptions.ClientId}&response_type=code&scope=openid email profile&redirect_uri={Uri.EscapeDataString(redirectUri)}&state={Uri.EscapeDataString(state)}&nonce={Uri.EscapeDataString(nonce)}&prompt=select_account";
            var url = $"{googleOptions.Authority.TrimEnd('/')}/o/oauth2/v2/auth{queryStr}";

            return Results.Redirect(url);
        });

        group.MapGet("/callback", async (HttpContext httpContext, [FromQuery] string code, [FromQuery] string state, IOptions<SsoOptions> options, SsoStore store, IRefreshTokenStore refreshTokens, UserManager<ApplicationUser> userManager, IJwtTokenGenerator tokenGenerator, ISecurityAuditService audit, IHttpClientFactory httpClientFactory) =>
        {
            var googleOptions = options.Value.Providers.FirstOrDefault(p => p.Name.Equals("Google", StringComparison.OrdinalIgnoreCase));
            if (googleOptions is null || !googleOptions.Enabled)
            {
                return Results.BadRequest(new SsoErrorResponse("provider_disabled", "Google SSO is not enabled."));
            }

            httpContext.Request.Cookies.TryGetValue("sso_state", out var savedState);
            httpContext.Request.Cookies.TryGetValue("sso_nonce", out var savedNonce);

            httpContext.Response.Cookies.Delete("sso_state");
            httpContext.Response.Cookies.Delete("sso_nonce");

            if (string.IsNullOrEmpty(state) || state != savedState)
            {
                return Results.BadRequest(new SsoErrorResponse("invalid_state", "Invalid or missing state parameter."));
            }

            if (string.IsNullOrEmpty(code))
            {
                return Results.BadRequest(new SsoErrorResponse("invalid_code", "Authorization code is missing."));
            }

            var redirectUri = GetRedirectUri(googleOptions, httpContext);

            // Exchange code for token
            var tokenEndpoint = "https://oauth2.googleapis.com/token";
            var httpClient = httpClientFactory.CreateClient();
            var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = googleOptions.ClientId,
                ["client_secret"] = googleOptions.ClientSecret,
                ["code"] = code,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = redirectUri
            });

            var tokenResponse = await httpClient.PostAsync(tokenEndpoint, tokenRequest);
            if (!tokenResponse.IsSuccessStatusCode)
            {
                var errorContent = await tokenResponse.Content.ReadAsStringAsync();
                audit.LogAuthEvent(SecurityAuditEvents.SsoCallback, "unknown", null, httpContext.Connection.RemoteIpAddress?.ToString(), httpContext.Request.Headers.UserAgent, new { status = "failed", provider = "Google", reason = "token_exchange_failed" });
                return Results.BadRequest(new SsoErrorResponse("token_exchange_failed", "Failed to exchange authorization code for tokens."));
            }

            var tokenResponseJson = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
            if (!tokenResponseJson.TryGetProperty("id_token", out var idTokenElement))
            {
                return Results.BadRequest(new SsoErrorResponse("missing_id_token", "Id token is missing from the response."));
            }

            var idToken = idTokenElement.GetString();
            if (string.IsNullOrEmpty(idToken))
            {
                return Results.BadRequest(new SsoErrorResponse("missing_id_token", "Id token is missing from the response."));
            }

            // Decode and extract claims from id_token
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(idToken))
            {
                return Results.BadRequest(new SsoErrorResponse("invalid_id_token", "Invalid Id token format."));
            }

            var jwtToken = handler.ReadJwtToken(idToken);
            
            // Validate nonce if present in ID token
            var nonceClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nonce")?.Value;
            if (!string.IsNullOrEmpty(nonceClaim) && nonceClaim != savedNonce)
            {
                return Results.BadRequest(new SsoErrorResponse("invalid_nonce", "Invalid nonce parameter."));
            }

            var subject = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var name = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var picture = jwtToken.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;

            if (string.IsNullOrEmpty(subject))
            {
                return Results.BadRequest(new SsoErrorResponse("missing_subject", "Id token does not contain a subject."));
            }

            // --- Persist user in Identity database (same pattern as Microsoft SSO) ---
            ApplicationUser? appUser = await userManager.FindByLoginAsync("Google", subject);
            if (appUser == null)
            {
                appUser = await userManager.FindByEmailAsync(email ?? string.Empty);
                if (appUser == null)
                {
                    appUser = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        DisplayName = name ?? email ?? "Google User",
                        Status = AccountStatus.Active,
                        EmailConfirmed = true
                    };

                    var createResult = await userManager.CreateAsync(appUser);
                    if (!createResult.Succeeded)
                    {
                        var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                        return Results.BadRequest(new SsoErrorResponse("user_creation_failed", $"Failed to create user account: {errors}"));
                    }
                }

                var linkResult = await userManager.AddLoginAsync(appUser, new UserLoginInfo("Google", subject, "Google"));
                if (!linkResult.Succeeded)
                {
                    var errors = string.Join("; ", linkResult.Errors.Select(e => e.Description));
                    return Results.BadRequest(new SsoErrorResponse("link_failed", $"Failed to link Google account: {errors}"));
                }
            }

            if (appUser.Status != AccountStatus.Active)
            {
                return Results.BadRequest(new SsoErrorResponse("account_inactive", $"Account is {appUser.Status}."));
            }

            // Generate token using the Identity module's JWT generator (same signing key, same claims)
            var authResponse = tokenGenerator.CreateToken(appUser);
            var refreshToken = refreshTokens.Issue(appUser.Id, appUser.Email ?? string.Empty);

            audit.LogAuthEvent(SecurityAuditEvents.SsoCallback, appUser.Id, appUser.Email, httpContext.Connection.RemoteIpAddress?.ToString(), httpContext.Request.Headers.UserAgent, new { status = "success", provider = "Google" });

            // Build SsoTokenResponse that the session-exchange endpoint expects
            var ssoUser = new SsoUserResponse(appUser.Id, appUser.Email, appUser.DisplayName, "User",
                new[] { new LinkedSsoProviderResponse("Google", subject, email, DateTimeOffset.UtcNow) });
            var authTokenResponse = new SsoTokenResponse(authResponse.AccessToken, "Bearer", 900, refreshToken, ssoUser);
            var sessionCode = store.CreatePendingSession(authTokenResponse);
            var redirectUrl = BuildFrontendCallbackRedirectUrl(googleOptions, sessionCode);
            return Results.Redirect(redirectUrl);
        });

        return app;
    }

    private static string GetRedirectUri(SsoProviderOptions options, HttpContext httpContext)
    {
        if (options.RedirectUrls != null && options.RedirectUrls.Count > 0)
        {
             // Use the first configured redirect URI, or match with incoming host if needed
             // Assuming the first one is the intended API callback or front-end callback
             // But since we are handling callback at /api/auth/sso/google/callback, we need to ensure the redirect URI matches exactly what's registered in Google.
             // We can construct it based on current request host.
             var request = httpContext.Request;
             return $"{request.Scheme}://{request.Host}/api/auth/sso/google/callback";
        }
        
        var req = httpContext.Request;
        return $"{req.Scheme}://{req.Host}/api/auth/sso/google/callback";
    }

    private static string BuildFrontendCallbackRedirectUrl(SsoProviderOptions options, string sessionCode)
    {
        var baseRedirectUrl = options.RedirectUrls.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(baseRedirectUrl))
        {
            throw new InvalidOperationException("Google SSO redirect URL is not configured.");
        }

        var builder = new UriBuilder(baseRedirectUrl);
        var query = new Dictionary<string, string>
        {
            ["provider"] = "google",
            ["sessionCode"] = sessionCode
        };

        builder.Query = string.Join("&", query.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        return builder.Uri.ToString();
    }

}
