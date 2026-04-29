using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NotesCool.Api.Configuration;
using NotesCool.Shared.Auth;
using NotesCool.Shared.Security;

namespace NotesCool.Api.Identity;

public static class SsoEndpoints
{
    public static IEndpointRouteBuilder MapSsoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/sso").WithTags("SSO");

        group.MapPost("/callback", Results<Ok<SsoTokenResponse>, BadRequest<SsoErrorResponse>> (SsoCallbackRequest request, SsoStore store, IConfiguration config, ISecurityAuditService audit, HttpContext http) =>
        {
            var providerUserId = request.ProviderUserId ?? request.Email;
            if (!SsoStore.IsValidCallback(request.Provider, request.Code, request.State, providerUserId ?? string.Empty))
            {
                audit.LogAuthEvent(SecurityAuditEvents.SsoCallback, "unknown", request.Email, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "failed", provider = request.Provider, reason = "invalid_callback" });
                return TypedResults.BadRequest(new SsoErrorResponse("invalid_sso_callback", "Invalid provider, state or authorization code."));
            }

            var user = store.GetOrCreateUser(request.Provider, providerUserId!, request.Email, request.DisplayName);
            audit.LogAuthEvent(SecurityAuditEvents.SsoCallback, user.UserId, user.Email, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "success", provider = request.Provider });
            
            return TypedResults.Ok(CreateTokenResponse(user, config));
        });

        group.MapGet("/providers", Ok<IReadOnlyCollection<SsoAvailableProviderResponse>> (IConfiguration config, IOptions<SsoOptions> ssoOptions) =>
        {
            var googleEnabled = config["AUTH_GOOGLE_ENABLED"] == "true" ||
                (ssoOptions.Value.Providers.FirstOrDefault(p => p.Name.Equals("Google", StringComparison.OrdinalIgnoreCase))?.Enabled ?? false);
            
            var microsoftEnabled = config["AUTH_MICROSOFT_ENABLED"] == "true" ||
                (ssoOptions.Value.Providers.FirstOrDefault(p => p.Name.Equals("Microsoft", StringComparison.OrdinalIgnoreCase))?.Enabled ?? false);

            var availableProviders = new List<SsoAvailableProviderResponse>();
            if (googleEnabled)
            {
                var googleOpt = ssoOptions.Value.Providers.FirstOrDefault(p => p.Name.Equals("Google", StringComparison.OrdinalIgnoreCase));
                availableProviders.Add(new SsoAvailableProviderResponse(
                    Key: "google",
                    DisplayName: "Google",
                    Icon: "google-icon",
                    Enabled: true,
                    LoginUrl: googleOpt?.CallbackPath ?? "/signin-google"
                ));
            }

            if (microsoftEnabled)
            {
                var microsoftOpt = ssoOptions.Value.Providers.FirstOrDefault(p => p.Name.Equals("Microsoft", StringComparison.OrdinalIgnoreCase));
                availableProviders.Add(new SsoAvailableProviderResponse(
                    Key: "microsoft",
                    DisplayName: "Microsoft",
                    Icon: "microsoft-icon",
                    Enabled: true,
                    LoginUrl: microsoftOpt?.CallbackPath ?? "/signin-microsoft"
                ));
            }

            return TypedResults.Ok<IReadOnlyCollection<SsoAvailableProviderResponse>>(availableProviders);
        });

        group.MapGet("/me/providers", Ok<IReadOnlyCollection<LinkedSsoProviderResponse>> (ICurrentUser currentUser, SsoStore store) =>
            TypedResults.Ok(store.GetProviders(currentUser.UserId))).RequireAuthorization();

        group.MapPost("/me/providers", Results<Ok<SsoUserResponse>, BadRequest<SsoErrorResponse>, Conflict<SsoErrorResponse>> (LinkSsoProviderRequest request, ICurrentUser currentUser, SsoStore store, ISecurityAuditService audit, HttpContext http) =>
        {
            if (!SsoStore.IsValidCallback(request.Provider, request.Code, request.State, request.ProviderUserId))
            {
                audit.LogAuthEvent(SecurityAuditEvents.SsoLink, currentUser.UserId, request.Email, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "failed", provider = request.Provider, reason = "invalid_callback" });
                return TypedResults.BadRequest(new SsoErrorResponse("invalid_sso_link", "Invalid provider, state or authorization code."));
            }

            try
            {
                var user = store.LinkProvider(currentUser.UserId, request.Provider, request.ProviderUserId, request.Email, request.DisplayName);
                audit.LogAuthEvent(SecurityAuditEvents.SsoLink, currentUser.UserId, request.Email, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "success", provider = request.Provider });
                return TypedResults.Ok(ToUserResponse(user));
            }
            catch (InvalidOperationException ex)
            {
                audit.LogAuthEvent(SecurityAuditEvents.SsoLink, currentUser.UserId, request.Email, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "failed", provider = request.Provider, reason = "identity_in_use" });
                return TypedResults.Conflict(new SsoErrorResponse("provider_identity_in_use", ex.Message));
            }
        }).RequireAuthorization();

        group.MapDelete("/me/providers/{provider}", Results<NoContent, BadRequest<SsoErrorResponse>> (string provider, ICurrentUser currentUser, SsoStore store, ISecurityAuditService audit, HttpContext http) =>
        {
            if (!store.TryUnlinkProvider(currentUser.UserId, provider, out var error))
            {
                audit.LogAuthEvent(SecurityAuditEvents.SsoUnlink, currentUser.UserId, null, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "failed", provider, reason = error });
                return TypedResults.BadRequest(new SsoErrorResponse("sso_unlink_denied", error ?? "Unable to unlink provider."));
            }

            audit.LogAuthEvent(SecurityAuditEvents.SsoUnlink, currentUser.UserId, null, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "success", provider });
            return TypedResults.NoContent();
        }).RequireAuthorization();


        return app;
    }

    private static SsoTokenResponse CreateTokenResponse(SsoUserRecord user, IConfiguration config)
    {
        var key = config["Jwt:Key"] ?? "development-only-notescool-sso-signing-key";
        var issuer = config["Jwt:Issuer"] ?? "NotesCool";
        var audience = config["Jwt:Audience"] ?? "NotesCool";
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key.PadRight(32, '0')));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(1);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId),
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role)
            },
            expires: expires,
            signingCredentials: credentials);

        return new SsoTokenResponse(new JwtSecurityTokenHandler().WriteToken(token), "Bearer", 3600, ToUserResponse(user));
    }

    private static SsoUserResponse ToUserResponse(SsoUserRecord user)
    {
        var providers = user.LinkedProviders.Values
            .Select(link => new LinkedSsoProviderResponse(link.Provider, link.ProviderUserId, link.Email, link.LinkedAt))
            .ToArray();

        return new SsoUserResponse(user.UserId, user.Email, user.DisplayName, user.Role, providers);
    }
}