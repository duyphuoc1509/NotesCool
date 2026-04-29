using System.Collections.Concurrent;
using NotesCool.Shared.Auth;

namespace NotesCool.Api.Identity;

public sealed class SsoStore
{
    private readonly ConcurrentDictionary<string, SsoUserRecord> _users = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, string> _providerIndex = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, SsoStateRecord> _states = new(StringComparer.Ordinal);

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

    public string CreateState(string provider, string returnUrl)
    {
        // Simple periodic cleanup of expired states to prevent memory leaks
        if (_states.Count > 1000)
        {
            foreach (var kvp in _states)
            {
                if (kvp.Value.ExpiresAt < DateTimeOffset.UtcNow)
                {
                    _states.TryRemove(kvp.Key, out _);
                }
            }
        }

        var state = "sso_" + Guid.NewGuid().ToString("N");
        _states[state] = new SsoStateRecord(Normalize(provider), returnUrl.Trim(), DateTimeOffset.UtcNow.AddMinutes(15));
        return state;
    }

    public bool ValidateAndConsumeState(string state, string provider, out string returnUrl)
    {
        returnUrl = string.Empty;

        if (string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(provider))
        {
            return false;
        }

        if (!_states.TryRemove(state, out var record))
        {
            return false;
        }

        if (record.ExpiresAt <= DateTimeOffset.UtcNow || !string.Equals(record.Provider, Normalize(provider), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        returnUrl = record.ReturnUrl;
        return true;
    }

    public bool IsValidCallback(string provider, string code, string state, string providerUserId)
        => !string.IsNullOrWhiteSpace(provider)
           && !string.IsNullOrWhiteSpace(code)
           && !string.IsNullOrWhiteSpace(providerUserId)
           && ValidateAndConsumeState(state, provider, out _);

    private static string BuildProviderKey(string provider, string providerUserId) => $"{Normalize(provider)}:{providerUserId.Trim()}";
    private static string Normalize(string provider) => provider.Trim().ToLowerInvariant();

    private sealed record SsoStateRecord(string Provider, string ReturnUrl, DateTimeOffset ExpiresAt);
}

public sealed record SsoProviderLink(string Provider, string ProviderUserId, string? Email, DateTimeOffset LinkedAt);

public sealed class SsoUserRecord
{
    public SsoUserRecord(string userId, string? email, string? displayName, string role = SystemRoles.User)
    {
        UserId = userId;
        Email = email;
        DisplayName = displayName;
        Role = role;
    }

    public string UserId { get; }
    public string? Email { get; }
    public string? DisplayName { get; }
    public string Role { get; }
    public ConcurrentDictionary<string, SsoProviderLink> LinkedProviders { get; } = new(StringComparer.OrdinalIgnoreCase);
}