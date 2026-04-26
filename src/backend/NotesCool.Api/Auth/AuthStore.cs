using System.Collections.Concurrent;

namespace NotesCool.Api.Auth;

public sealed class AuthStore
{
    private readonly ConcurrentDictionary<string, RefreshTokenRecord> _refreshTokens = new(StringComparer.Ordinal);

    public AuthSession CreateSession(string email, int? refreshTokenLifetimeSeconds = null)
    {
        var userId = NormalizeEmail(email);
        var refreshToken = Guid.NewGuid().ToString("N");
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(refreshTokenLifetimeSeconds ?? 7 * 24 * 60 * 60);

        var record = new RefreshTokenRecord(refreshToken, userId, expiresAt);
        _refreshTokens[refreshToken] = record;

        return new AuthSession(userId, refreshToken, expiresAt);
    }

    public bool TryRotateRefreshToken(string refreshToken, int? refreshTokenLifetimeSeconds, out AuthSession? session, out RefreshTokenFailureReason failureReason)
    {
        session = null;

        if (!_refreshTokens.TryGetValue(refreshToken, out var existing))
        {
            failureReason = RefreshTokenFailureReason.Invalid;
            return false;
        }

        if (existing.IsRevoked)
        {
            failureReason = RefreshTokenFailureReason.Revoked;
            return false;
        }

        if (existing.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            existing.Revoke();
            failureReason = RefreshTokenFailureReason.Expired;
            return false;
        }

        existing.Revoke();
        session = CreateSession(existing.UserId, refreshTokenLifetimeSeconds);
        failureReason = RefreshTokenFailureReason.None;
        return true;
    }

    public bool TryRevokeRefreshToken(string refreshToken, out RefreshTokenFailureReason failureReason)
    {
        if (!_refreshTokens.TryGetValue(refreshToken, out var existing))
        {
            failureReason = RefreshTokenFailureReason.Invalid;
            return false;
        }

        if (existing.IsRevoked)
        {
            failureReason = RefreshTokenFailureReason.Revoked;
            return false;
        }

        if (existing.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            existing.Revoke();
            failureReason = RefreshTokenFailureReason.Expired;
            return false;
        }

        existing.Revoke();
        failureReason = RefreshTokenFailureReason.None;
        return true;
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}

public sealed record AuthSession(string UserId, string RefreshToken, DateTimeOffset RefreshTokenExpiresAt);

public enum RefreshTokenFailureReason
{
    None,
    Invalid,
    Revoked,
    Expired
}

public sealed class RefreshTokenRecord
{
    public RefreshTokenRecord(string token, string userId, DateTimeOffset expiresAt)
    {
        Token = token;
        UserId = userId;
        ExpiresAt = expiresAt;
    }

    public string Token { get; }
    public string UserId { get; }
    public DateTimeOffset ExpiresAt { get; }
    public bool IsRevoked { get; private set; }

    public void Revoke() => IsRevoked = true;
}
