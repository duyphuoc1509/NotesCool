using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NotesCool.Identity.Application;
using NotesCool.Identity.Infrastructure;
using NotesCool.Shared.Auth;
using NotesCool.Shared.Security;

namespace NotesCool.Api.Identity;

public static class SsoEndpoints
{
    public static IEndpointRouteBuilder MapSsoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/sso").WithTags("SSO");

        group.MapPost("/callback", async Task<Results<Ok<SsoTokenResponse>, BadRequest<SsoErrorResponse>>> (SsoCallbackRequest request, SsoService store, UserManager<ApplicationUser> userManager, IConfiguration config, ISecurityAuditService audit, HttpContext http) =>
        {
            var providerUserId = request.ProviderUserId ?? request.Email;
            if (!store.IsValidCallback(request.Provider, request.Code, request.State, providerUserId ?? string.Empty))
            {
                audit.LogAuthEvent(SecurityAuditEvents.SsoCallback, "unknown", request.Email, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "failed", provider = request.Provider, reason = "invalid_callback" });
                return TypedResults.BadRequest(new SsoErrorResponse("invalid_sso_callback", "Invalid provider, state or authorization code."));
            }

            var user = await store.GetOrCreateUserAsync(request.Provider, providerUserId!, request.Email, request.DisplayName);
            audit.LogAuthEvent(SecurityAuditEvents.SsoCallback, user.Id, user.Email, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "success", provider = request.Provider });
            
            var roles = await userManager.GetRolesAsync(user);
            var providers = await store.GetProvidersAsync(user.Id);
            return TypedResults.Ok(CreateTokenResponse(user, roles, providers, config));
        });

        group.MapGet("/providers", async Task<Ok<IReadOnlyCollection<LinkedSsoProviderResponse>>> (ICurrentUser currentUser, SsoService store) =>
        {
            var providers = await store.GetProvidersAsync(currentUser.UserId);
            var response = providers.Select(link => new LinkedSsoProviderResponse(link.ProviderKey, link.ProviderSubject, link.Email, link.CreatedAt)).ToArray();
            return TypedResults.Ok<IReadOnlyCollection<LinkedSsoProviderResponse>>(response);
        }).RequireAuthorization();

        group.MapPost("/providers", async Task<Results<Ok<SsoUserResponse>, BadRequest<SsoErrorResponse>, Conflict<SsoErrorResponse>>> (LinkSsoProviderRequest request, ICurrentUser currentUser, SsoService store, UserManager<ApplicationUser> userManager, ISecurityAuditService audit, HttpContext http) =>
        {
            if (!store.IsValidCallback(request.Provider, request.Code, request.State, request.ProviderUserId))
            {
                audit.LogAuthEvent(SecurityAuditEvents.SsoLink, currentUser.UserId, request.Email, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "failed", provider = request.Provider, reason = "invalid_callback" });
                return TypedResults.BadRequest(new SsoErrorResponse("invalid_sso_link", "Invalid provider, state or authorization code."));
            }

            try
            {
                var user = await store.LinkProviderAsync(currentUser.UserId, request.Provider, request.ProviderUserId, request.Email, request.DisplayName);
                audit.LogAuthEvent(SecurityAuditEvents.SsoLink, currentUser.UserId, request.Email, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "success", provider = request.Provider });
                var roles = await userManager.GetRolesAsync(user);
                var providers = await store.GetProvidersAsync(user.Id);
                return TypedResults.Ok(ToUserResponse(user, roles, providers));
            }
            catch (InvalidOperationException ex)
            {
                audit.LogAuthEvent(SecurityAuditEvents.SsoLink, currentUser.UserId, request.Email, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "failed", provider = request.Provider, reason = "identity_in_use" });
                return TypedResults.Conflict(new SsoErrorResponse("provider_identity_in_use", ex.Message));
            }
        }).RequireAuthorization();

        group.MapDelete("/providers/{provider}", async Task<Results<NoContent, BadRequest<SsoErrorResponse>>> (string provider, ICurrentUser currentUser, SsoService store, ISecurityAuditService audit, HttpContext http) =>
        {
            var (success, error) = await store.TryUnlinkProviderAsync(currentUser.UserId, provider);
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

    private static SsoTokenResponse CreateTokenResponse(ApplicationUser user, IList<string> roles, IReadOnlyCollection<UserExternalLogin> providers, IConfiguration config)
    {
        var key = config["Jwt:Key"] ?? "development-only-notescool-sso-signing-key";
        var issuer = config["Jwt:Issuer"] ?? "NotesCool";
        var audience = config["Jwt:Audience"] ?? "NotesCool";
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key.PadRight(32, '0')));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(1);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        var role = roles.FirstOrDefault() ?? SystemRoles.User;
        claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expires,
            signingCredentials: credentials);

        return new SsoTokenResponse(new JwtSecurityTokenHandler().WriteToken(token), "Bearer", 3600, ToUserResponse(user, roles, providers));
    }

    private static SsoUserResponse ToUserResponse(ApplicationUser user, IList<string> roles, IReadOnlyCollection<UserExternalLogin> providers)
    {
        var providerResponses = providers
            .Select(link => new LinkedSsoProviderResponse(link.ProviderKey, link.ProviderSubject, link.Email, link.CreatedAt))
            .ToArray();

        var role = roles.FirstOrDefault() ?? SystemRoles.User;
        return new SsoUserResponse(user.Id, user.Email, user.DisplayName, role, providerResponses);
    }
}