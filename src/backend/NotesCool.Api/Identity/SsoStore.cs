using System.Collections.Concurrent;

namespace NotesCool.Api.Identity;

public sealed class SsoStore
{
    private readonly ConcurrentDictionary<string, SsoUserRecord> _users = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, string> _providerIndex = new(StringComparer.OrdinalIgnoreCase);

    public SsoUserRecord GetOrCreateUser(string provider, string providerUserId, string? email, string? displayName)
    {
        var key = BuildProviderKey(provider, providerUserId);
        if (_providerIndex.TryGetValue(key, out var existingUserId) && _users.TryGetValue(existingUserId, out var existingUser))
        {
            return existingUser;
        }

        var userId = string.IsNullOrWhiteSpace(email) ? Guid.NewGuid().ToString("N") : email.Trim().ToLowerInvariant();
        var user = _users.GetOrAdd(userId, id => new SsoUserRecord(id, email, displayName));
        LinkProvider(user.UserId, provider, providerUserId, email, displayName);
        return user;
    }

    public SsoUserRecord LinkProvider(string userId, string provider, string providerUserId, string? email, string? displayName)
    {
        var key = BuildProviderKey(provider, providerUserId);
        if (_providerIndex.TryGetValue(key, out var linkedUserId) && !string.Equals(linkedUserId, userId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Provider identity is already linked to another account.");
        }

        var user = _users.GetOrAdd(userId, id => new SsoUserRecord(id, email, displayName));
        user.LinkedProviders[Normalize(provider)] = new SsoProviderLink(Normalize(provider), providerUserId.Trim(), email, DateTimeOffset.UtcNow);
        _providerIndex[key] = user.UserId;
        return user;
    }

    public bool TryUnlinkProvider(string userId, string provider, out string? error)
    {
        error = null;
        if (!_users.TryGetValue(userId, out var user))
        {
            error = "User was not found.";
            return false;
        }

        var normalizedProvider = Normalize(provider);
        if (!user.LinkedProviders.TryGetValue(normalizedProvider, out var link))
        {
            error = "Provider is not linked to this account.";
            return false;
        }

        if (user.LinkedProviders.Count <= 1)
        {
            error = "At least one login method must remain linked.";
            return false;
        }

        user.LinkedProviders.TryRemove(normalizedProvider, out _);
        _providerIndex.TryRemove(BuildProviderKey(normalizedProvider, link.ProviderUserId), out _);
        return true;
    }

    public IReadOnlyCollection<LinkedSsoProviderResponse> GetProviders(string userId)
    {
        return _users.TryGetValue(userId, out var user)
            ? user.LinkedProviders.Values.Select(link => new LinkedSsoProviderResponse(link.Provider, link.ProviderUserId, link.Email, link.LinkedAt)).ToArray()
            : Array.Empty<LinkedSsoProviderResponse>();
    }

    public static bool IsValidCallback(string provider, string code, string state, string providerUserId)
        => !string.IsNullOrWhiteSpace(provider)
           && !string.IsNullOrWhiteSpace(code)
           && !string.IsNullOrWhiteSpace(state)
           && !string.IsNullOrWhiteSpace(providerUserId)
           && state.StartsWith("sso_", StringComparison.OrdinalIgnoreCase);

    private static string BuildProviderKey(string provider, string providerUserId) => $"{Normalize(provider)}:{providerUserId.Trim()}";
    private static string Normalize(string provider) => provider.Trim().ToLowerInvariant();
}

public sealed record SsoProviderLink(string Provider, string ProviderUserId, string? Email, DateTimeOffset LinkedAt);

public sealed class SsoUserRecord
{
    public SsoUserRecord(string userId, string? email, string? displayName)
    {
        UserId = userId;
        Email = email;
        DisplayName = displayName;
    }

    public string UserId { get; }
    public string? Email { get; }
    public string? DisplayName { get; }
    public ConcurrentDictionary<string, SsoProviderLink> LinkedProviders { get; } = new(StringComparer.OrdinalIgnoreCase);
}
