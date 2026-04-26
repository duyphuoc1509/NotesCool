using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NotesCool.Shared.Security;

namespace NotesCool.Api.Contracts;

public static class AuthEndpoints
{
    private const int AccessTokenExpiresInSeconds = 900;

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", (LoginRequest request, IRefreshTokenStore refreshTokens, IConfiguration configuration, ISecurityAuditService audit, HttpContext http) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                audit.LogAuthEvent(SecurityAuditEvents.LoginFailed, "unknown", request.Email, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { reason = "missing_credentials" });
                return Results.BadRequest(new { message = "Email and password are required." });
            }

            var userId = request.Email.Trim().ToLowerInvariant();
            var response = CreateTokenResponse(userId, request.Email.Trim(), refreshTokens, configuration);
            
            audit.LogAuthEvent(SecurityAuditEvents.LoginSuccess, userId, request.Email.Trim(), http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent);
            
            return Results.Ok(response);
        });

        group.MapPost("/refresh", (RefreshTokenRequest request, IRefreshTokenStore refreshTokens, IConfiguration configuration, ISecurityAuditService audit, HttpContext http) =>
        {
            if (!refreshTokens.TryRevoke(request.RefreshToken, out var session))
            {
                audit.LogAuthEvent(SecurityAuditEvents.RefreshToken, "unknown", null, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "failed", reason = "invalid_token" });
                return Results.Unauthorized();
            }

            var response = CreateTokenResponse(session.UserId, session.Email, refreshTokens, configuration);
            
            audit.LogAuthEvent(SecurityAuditEvents.RefreshToken, session.UserId, session.Email, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "success" });
            
            return Results.Ok(response);
        });

        group.MapPost("/logout", (RefreshTokenRequest request, IRefreshTokenStore refreshTokens, ISecurityAuditService audit, HttpContext http) =>
        {
            if (refreshTokens.TryRevoke(request.RefreshToken, out var session))
            {
                audit.LogAuthEvent(SecurityAuditEvents.Logout, session.UserId, session.Email, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent);
            }
            
            return Results.NoContent();
        });

        return app;
    }

    private static AuthResponse CreateTokenResponse(string userId, string email, IRefreshTokenStore refreshTokens, IConfiguration configuration)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(AccessTokenExpiresInSeconds);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetSigningKey(configuration)));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"] ?? "NotesCool",
            audience: configuration["Jwt:Audience"] ?? "NotesCool",
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, "User")
            ],
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = refreshTokens.Issue(userId, email);

        return new AuthResponse(accessToken, refreshToken, "Bearer", AccessTokenExpiresInSeconds, expiresAt);
    }

    private static string GetSigningKey(IConfiguration configuration) =>
        configuration["Jwt:SigningKey"] ?? "NotesCool development signing key with at least 32 chars";
}

public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshTokenRequest(string RefreshToken);

/// <summary>
/// Access tokens are bearer JWTs that remain valid until AccessTokenExpiresAtUtc
/// (AccessTokenExpiresInSeconds seconds after issuance). Logout revokes only the
/// current refresh token/session, so clients should discard the access token locally.
/// </summary>
public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int AccessTokenExpiresInSeconds,
    DateTimeOffset AccessTokenExpiresAtUtc);

public sealed record RefreshTokenSession(string UserId, string Email, DateTimeOffset CreatedAtUtc, bool IsRevoked);

public interface IRefreshTokenStore
{
    string Issue(string userId, string email);
    bool TryRevoke(string refreshToken, out RefreshTokenSession session);
}

public sealed class InMemoryRefreshTokenStore : IRefreshTokenStore
{
    private readonly ConcurrentDictionary<string, RefreshTokenSession> _sessions = new();

    public string Issue(string userId, string email)
    {
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        _sessions[refreshToken] = new RefreshTokenSession(userId, email, DateTimeOffset.UtcNow, false);
        return refreshToken;
    }

    public bool TryRevoke(string refreshToken, out RefreshTokenSession session)
    {
        session = default!;
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return false;
        }

        if (!_sessions.TryGetValue(refreshToken, out var existing) || existing.IsRevoked)
        {
            return false;
        }

        var revoked = existing with { IsRevoked = true };
        if (!_sessions.TryUpdate(refreshToken, revoked, existing))
        {
            return false;
        }

        session = existing;
        return true;
    }
}
