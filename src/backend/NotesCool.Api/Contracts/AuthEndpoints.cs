using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using NotesCool.Identity.Application;
using NotesCool.Identity.Application.Abstractions;
using NotesCool.Identity.Infrastructure;
using NotesCool.Shared.Security;

namespace NotesCool.Api.Contracts;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (LoginRequest request, IRefreshTokenStore refreshTokens, AccountService accountService, IConfiguration configuration, ISecurityAuditService audit, HttpContext http) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                audit.LogAuthEvent(SecurityAuditEvents.LoginFailed, "unknown", request.Email, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { reason = "missing_credentials" });
                return Results.BadRequest(new { message = "Email and password are required." });
            }

            try
            {
                var identityRequest = new NotesCool.Identity.Contracts.LoginRequest(request.Email.Trim(), request.Password);
                var identityResponse = await accountService.LoginAsync(identityRequest);

                var userId = identityResponse.User.Id;
                var email = identityResponse.User.Email;
                var refreshToken = refreshTokens.Issue(userId, email);

                var expiresAt = new DateTimeOffset(DateTime.SpecifyKind(identityResponse.ExpiresAtUtc, DateTimeKind.Utc));
                var expiresInSeconds = (int)Math.Max(0, (expiresAt - DateTimeOffset.UtcNow).TotalSeconds);

                var user = new AuthUserResponse(
                    identityResponse.User.Id,
                    identityResponse.User.Email,
                    identityResponse.User.DisplayName,
                    identityResponse.User.Status,
                    identityResponse.User.Roles.ToArray());

                audit.LogAuthEvent(SecurityAuditEvents.LoginSuccess, userId, email, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent);

                return Results.Ok(new AuthResponse(identityResponse.AccessToken, refreshToken, "Bearer", expiresInSeconds, expiresAt, user));
            }
            catch (UnauthorizedAccessException)
            {
                audit.LogAuthEvent(SecurityAuditEvents.LoginFailed, "unknown", request.Email?.Trim(), http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { reason = "invalid_credentials" });
                return Results.Unauthorized();
            }
            catch (AccountInactiveException)
            {
                audit.LogAuthEvent(SecurityAuditEvents.LoginFailed, "unknown", request.Email?.Trim(), http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { reason = "account_inactive" });
                return Results.Json(new { message = "Account is not active." }, statusCode: 403);
            }
        });

        group.MapPost("/refresh", async (RefreshTokenRequest request, IRefreshTokenStore refreshTokens, UserManager<ApplicationUser> userManager, IJwtTokenGenerator tokenGenerator, ISecurityAuditService audit, HttpContext http) =>
        {
            if (!refreshTokens.TryRevoke(request.RefreshToken, out var session))
            {
                audit.LogAuthEvent(SecurityAuditEvents.RefreshToken, "unknown", null, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "failed", reason = "invalid_token" });
                return Results.Unauthorized();
            }

            var appUser = await userManager.FindByIdAsync(session.UserId);
            if (appUser is null)
            {
                audit.LogAuthEvent(SecurityAuditEvents.RefreshToken, session.UserId, session.Email, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "failed", reason = "user_not_found" });
                return Results.Unauthorized();
            }

            if (appUser.Status != AccountStatus.Active)
            {
                audit.LogAuthEvent(SecurityAuditEvents.RefreshToken, session.UserId, session.Email, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "failed", reason = "account_inactive" });
                return Results.Json(new { message = "Account is not active." }, statusCode: 403);
            }

            var roles = await userManager.GetRolesAsync(appUser);
            var identityResponse = tokenGenerator.CreateToken(appUser, roles);
            var refreshToken = refreshTokens.Issue(session.UserId, session.Email ?? string.Empty);

            var expiresAt = new DateTimeOffset(DateTime.SpecifyKind(identityResponse.ExpiresAtUtc, DateTimeKind.Utc));
            var expiresInSeconds = (int)Math.Max(0, (expiresAt - DateTimeOffset.UtcNow).TotalSeconds);

            var user = new AuthUserResponse(
                identityResponse.User.Id,
                identityResponse.User.Email,
                identityResponse.User.DisplayName,
                identityResponse.User.Status,
                identityResponse.User.Roles.ToArray());

            audit.LogAuthEvent(SecurityAuditEvents.RefreshToken, session.UserId, session.Email, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent, new { status = "success" });

            return Results.Ok(new AuthResponse(identityResponse.AccessToken, refreshToken, "Bearer", expiresInSeconds, expiresAt, user));
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
}

public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshTokenRequest(string RefreshToken);

/// <summary>
/// Access tokens are bearer JWTs that remain valid until AccessTokenExpiresAtUtc
/// (see <see cref="AuthResponse.AccessTokenExpiresInSeconds" />). Logout revokes only the
/// current refresh token/session, so clients should discard the access token locally.
/// </summary>
public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int AccessTokenExpiresInSeconds,
    DateTimeOffset AccessTokenExpiresAtUtc,
    AuthUserResponse? User = null);

public sealed record AuthUserResponse(
    string Id,
    string Email,
    string DisplayName,
    string Status,
    string[] Roles);

public sealed record RefreshTokenSession(string UserId, string Email, DateTimeOffset CreatedAtUtc, DateTimeOffset ExpiresAtUtc, bool IsRevoked);

public interface IRefreshTokenStore
{
    string Issue(string userId, string email);
    bool TryRevoke(string refreshToken, out RefreshTokenSession session);
}

public sealed class InMemoryRefreshTokenStore : IRefreshTokenStore
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromDays(7);

    private readonly ConcurrentDictionary<string, RefreshTokenSession> _sessions = new();
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _ttl;

    public InMemoryRefreshTokenStore() : this(TimeProvider.System, DefaultTtl) { }

    public InMemoryRefreshTokenStore(TimeProvider timeProvider, TimeSpan? ttl = null)
    {
        _timeProvider = timeProvider;
        _ttl = ttl ?? DefaultTtl;
    }

    public string Issue(string userId, string email)
    {
        var now = _timeProvider.GetUtcNow();
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        _sessions[refreshToken] = new RefreshTokenSession(userId, email, now, now.Add(_ttl), false);
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

        // Reject expired tokens
        if (_timeProvider.GetUtcNow() > existing.ExpiresAtUtc)
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
