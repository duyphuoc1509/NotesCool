using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NotesCool.Api.Auth;
using NotesCool.Api.Contracts;
using NotesCool.Api.Configuration;
using NotesCool.Shared.Auth;
using NotesCool.Shared.Security;

namespace NotesCool.Api.Identity;

public static class SsoEndpoints
{
    public static IEndpointRouteBuilder MapSsoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/sso").WithTags("SSO");

        group.MapGet("/providers", Ok<IReadOnlyCollection<SsoAvailableProviderResponse>> (IOptions<SsoOptions> ssoOptions) =>
        {
            var providers = ssoOptions.Value.Providers
                .Where(p => p.Enabled)
                .Select(p =>
                {
                    var key = p.Name.ToLowerInvariant();
                    return new SsoAvailableProviderResponse(
                        Key: key,
                        DisplayName: p.Name,
                        Icon: key,
                        Enabled: p.Enabled,
                        LoginUrl: $"/api/auth/sso/{key}/login");
                })
                .ToArray();

            return TypedResults.Ok((IReadOnlyCollection<SsoAvailableProviderResponse>)providers);
        });

        group.MapPost("/callback", Results<Ok<SsoTokenResponse>, BadRequest<SsoErrorResponse>> (SsoCallbackRequest request, SsoStore store, IRefreshTokenStore refreshTokens, IOptions<SsoOptions> ssoOptions, IConfiguration config, ISecurityAuditService audit, HttpContext http) =>
        {
            var providerUserId = request.ProviderUserId ?? request.Email;
            if (!store.IsValidCallback(request.Provider, request.Code, request.State, providerUserId ?? string.Empty))
            {
                audit.LogAuthEvent(SecurityAuditEvents.SsoCallback, "unknown", request.Email, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "failed", provider = request.Provider, reason = "invalid_callback" });
                return TypedResults.BadRequest(new SsoErrorResponse("invalid_sso_callback", "Invalid provider, state or authorization code."));
            }

            var providerOptions = ssoOptions.Value.Providers.FirstOrDefault(p => string.Equals(p.Name, request.Provider, StringComparison.OrdinalIgnoreCase));
            if (providerOptions is null || !providerOptions.Enabled)
            {
                audit.LogAuthEvent(SecurityAuditEvents.SsoCallback, "unknown", request.Email, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "failed", provider = request.Provider, reason = "provider_disabled" });
                return TypedResults.BadRequest(new SsoErrorResponse("provider_disabled", "The specified SSO provider is not enabled."));
            }

            var user = store.GetOrCreateUser(request.Provider, providerUserId!, request.Email, request.DisplayName);
            var refreshToken = refreshTokens.Issue(user.UserId, user.Email ?? string.Empty);
            audit.LogAuthEvent(SecurityAuditEvents.SsoCallback, user.UserId, user.Email, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "success", provider = request.Provider });
            
            return TypedResults.Ok(CreateTokenResponse(user, refreshToken, config));
        });

        group.MapPost("/session", Results<Ok<SsoTokenResponse>, BadRequest<SsoErrorResponse>> (SsoSessionExchangeRequest request, SsoStore store) =>
        {
            if (store.TryConsumePendingSession(request.Code, out var tokenResponse))
            {
                return TypedResults.Ok(tokenResponse);
            }
            return TypedResults.BadRequest(new SsoErrorResponse("invalid_session_code", "Invalid or expired session code."));
        });

        group.MapGet("/me/providers", Ok<IReadOnlyCollection<LinkedSsoProviderResponse>> (ICurrentUser currentUser, SsoStore store) =>
            TypedResults.Ok(store.GetProviders(currentUser.UserId))).RequireAuthorization();

        group.MapPost("/me/providers", Results<Ok<SsoUserResponse>, BadRequest<SsoErrorResponse>, Conflict<SsoErrorResponse>> (LinkSsoProviderRequest request, ICurrentUser currentUser, SsoStore store, IOptions<SsoOptions> ssoOptions, ISecurityAuditService audit, HttpContext http) =>
        {
            var providerOptions = ssoOptions.Value.Providers.FirstOrDefault(p => string.Equals(p.Name, request.Provider, StringComparison.OrdinalIgnoreCase));
            if (providerOptions is null || !providerOptions.Enabled)
            {
                audit.LogAuthEvent(SecurityAuditEvents.SsoLink, currentUser.UserId, request.Email, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "failed", provider = request.Provider, reason = "provider_disabled" });
                return TypedResults.BadRequest(new SsoErrorResponse("provider_disabled", "The specified SSO provider is not enabled."));
            }

            if (!store.IsValidCallback(request.Provider, request.Code, request.State, request.ProviderUserId))
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
            var success = store.TryUnlinkProvider(currentUser.UserId, provider, out var error);
            if (!success)
            {
                audit.LogAuthEvent(SecurityAuditEvents.SsoUnlink, currentUser.UserId, null, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "failed", provider, reason = error });
                return TypedResults.BadRequest(new SsoErrorResponse("sso_unlink_denied", error ?? "Unable to unlink provider."));
            }

            audit.LogAuthEvent(SecurityAuditEvents.SsoUnlink, currentUser.UserId, null, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "success", provider });
            return TypedResults.NoContent();
        }).RequireAuthorization();


        return app;
    }

    private static SsoTokenResponse CreateTokenResponse(SsoUserRecord user, string refreshToken, IConfiguration config)
    {
        var key = config["Jwt:SigningKey"] ?? "NotesCool development signing key with at least 32 chars";
        var issuer = config["Jwt:Issuer"] ?? "NotesCool";
        var audience = config["Jwt:Audience"] ?? "NotesCool";
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(15);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId),
            new Claim(ClaimTypes.NameIdentifier, user.UserId),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        var role = user.Role;
        claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expires,
            signingCredentials: credentials);

        return new SsoTokenResponse(new JwtSecurityTokenHandler().WriteToken(token), "Bearer", 900, refreshToken, ToUserResponse(user));
    }

    private static SsoUserResponse ToUserResponse(SsoUserRecord user)
    {
        var providerResponses = user.LinkedProviders.Values
            .Select(link => new LinkedSsoProviderResponse(link.Provider, link.ProviderUserId, link.Email, link.LinkedAt))
            .ToArray();

        return new SsoUserResponse(user.UserId, user.Email, user.DisplayName, user.Role, providerResponses);
    }
}