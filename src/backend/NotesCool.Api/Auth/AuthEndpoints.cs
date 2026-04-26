using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;

namespace NotesCool.Api.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", (LoginRequest request, AuthStore store, IConfiguration config) =>
        {
            // For now, any email/password is valid for testing purposes
            var session = store.CreateSession(request.Email, request.RefreshTokenLifetimeSeconds);
            return TypedResults.Ok(CreateTokenResponse(session, config));
        });

        group.MapPost("/refresh", Results<Ok<AuthTokenResponse>, UnauthorizedHttpResult> (RefreshTokenRequest request, AuthStore store, IConfiguration config) =>
        {
            if (store.TryRotateRefreshToken(request.RefreshToken, null, out var session, out _))
            {
                return TypedResults.Ok(CreateTokenResponse(session!, config));
            }

            return TypedResults.Unauthorized();
        });

        group.MapPost("/logout", Results<NoContent, UnauthorizedHttpResult> (RefreshTokenRequest request, AuthStore store) =>
        {
            if (store.TryRevokeRefreshToken(request.RefreshToken, out _))
            {
                return TypedResults.NoContent();
            }

            return TypedResults.Unauthorized();
        });

        return app;
    }

    private static AuthTokenResponse CreateTokenResponse(AuthSession session, IConfiguration config)
    {
        var key = config["Jwt:Key"] ?? "development-only-notescool-auth-signing-key";
        var issuer = config["Jwt:Issuer"] ?? "NotesCool";
        var audience = config["Jwt:Audience"] ?? "NotesCool";
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key.PadRight(32, '0')));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(15);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, session.UserId),
                new Claim(ClaimTypes.NameIdentifier, session.UserId),
                new Claim(ClaimTypes.Email, session.UserId)
            },
            expires: expires,
            signingCredentials: credentials);

        return new AuthTokenResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            session.RefreshToken,
            "Bearer",
            900);
    }
}
