using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;
using NotesCool.Shared.Auth;

namespace NotesCool.Api.Identity;

public static class SsoEndpoints
{
    public static IEndpointRouteBuilder MapSsoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/sso").WithTags("SSO");

        group.MapPost("/callback", Results<Ok<SsoTokenResponse>, BadRequest<SsoErrorResponse>> (SsoCallbackRequest request, SsoStore store, IConfiguration config) =>
        {
            var providerUserId = request.ProviderUserId ?? request.Email;
            if (!SsoStore.IsValidCallback(request.Provider, request.Code, request.State, providerUserId ?? string.Empty))
            {
                return TypedResults.BadRequest(new SsoErrorResponse("invalid_sso_callback", "Invalid provider, state or authorization code."));
            }

            var user = store.GetOrCreateUser(request.Provider, providerUserId!, request.Email, request.DisplayName);
            return TypedResults.Ok(CreateTokenResponse(user, config));
        });

        group.MapGet("/providers", Ok<IReadOnlyCollection<LinkedSsoProviderResponse>> (ICurrentUser currentUser, SsoStore store) =>
            TypedResults.Ok(store.GetProviders(currentUser.UserId))).RequireAuthorization();

        group.MapPost("/providers", Results<Ok<SsoUserResponse>, BadRequest<SsoErrorResponse>, Conflict<SsoErrorResponse>> (LinkSsoProviderRequest request, ICurrentUser currentUser, SsoStore store) =>
        {
            if (!SsoStore.IsValidCallback(request.Provider, request.Code, request.State, request.ProviderUserId))
            {
                return TypedResults.BadRequest(new SsoErrorResponse("invalid_sso_link", "Invalid provider, state or authorization code."));
            }

            try
            {
                var user = store.LinkProvider(currentUser.UserId, request.Provider, request.ProviderUserId, request.Email, request.DisplayName);
                return TypedResults.Ok(ToUserResponse(user));
            }
            catch (InvalidOperationException ex)
            {
                return TypedResults.Conflict(new SsoErrorResponse("provider_identity_in_use", ex.Message));
            }
        }).RequireAuthorization();

        group.MapDelete("/providers/{provider}", Results<NoContent, BadRequest<SsoErrorResponse>> (string provider, ICurrentUser currentUser, SsoStore store) =>
        {
            if (!store.TryUnlinkProvider(currentUser.UserId, provider, out var error))
            {
                return TypedResults.BadRequest(new SsoErrorResponse("sso_unlink_denied", error ?? "Unable to unlink provider."));
            }

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
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
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

        return new SsoUserResponse(user.UserId, user.Email, user.DisplayName, providers);
    }
}