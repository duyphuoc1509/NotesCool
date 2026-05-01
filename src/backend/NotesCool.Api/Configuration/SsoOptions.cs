using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace NotesCool.Api.Configuration;

public sealed class SsoOptions
{
    public List<SsoProviderOptions> Providers { get; init; } = [];
}

public sealed class SsoProviderOptions
{
    public string Name { get; init; } = string.Empty;

    public bool Enabled { get; init; }

    public string ClientId { get; init; } = string.Empty;

    public string ClientSecret { get; init; } = string.Empty;

    public string Authority { get; init; } = string.Empty;

    public string CallbackPath { get; init; } = string.Empty;

    /// <summary>
    /// Absolute URL sent to the OAuth provider as <c>redirect_uri</c>. This MUST exactly match what
    /// is registered with the provider (e.g. Google Cloud Console). When empty, the API falls back
    /// to building the URL from the incoming request, which can produce wrong scheme (http vs https)
    /// behind reverse proxies that don't forward <c>X-Forwarded-Proto</c> correctly.
    /// </summary>
    public string RedirectUri { get; init; } = string.Empty;

    /// <summary>
    /// Whitelist of frontend URLs the API may redirect the browser to after a successful SSO
    /// exchange (the URL receives <c>?provider=...&amp;sessionCode=...</c>). NOT the OAuth
    /// <c>redirect_uri</c> — see <see cref="RedirectUri"/> for that.
    /// </summary>
    public List<string> RedirectUrls { get; init; } = [];
}

public sealed class SsoOptionsValidator(IHostEnvironment environment) : IValidateOptions<SsoOptions>
{
    private static readonly HashSet<string> SecretPlaceholders = new(StringComparer.OrdinalIgnoreCase)
    {
        "",
        "change-me",
        "replace-me",
        "placeholder",
        "__SET_IN_ENV__"
    };

    public ValidateOptionsResult Validate(string? name, SsoOptions options)
    {
        var failures = new List<string>();

        foreach (var provider in options.Providers.Where(provider => provider.Enabled))
        {
            var providerName = string.IsNullOrWhiteSpace(provider.Name) ? "<unnamed>" : provider.Name;
            ValidateEnabledProvider(provider, providerName, failures);
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }

    private void ValidateEnabledProvider(SsoProviderOptions provider, string providerName, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(provider.Name))
        {
            failures.Add("Each enabled SSO provider must define Sso:Providers:<index>:Name.");
        }

        if (string.IsNullOrWhiteSpace(provider.ClientId))
        {
            failures.Add($"SSO provider '{providerName}' is enabled but ClientId is not configured.");
        }

        if (IsInvalidSecret(provider.ClientSecret))
        {
            failures.Add($"SSO provider '{providerName}' is enabled but ClientSecret is empty or still uses a placeholder.");
        }

        if (!Uri.TryCreate(provider.Authority, UriKind.Absolute, out var authority) || authority.Scheme != Uri.UriSchemeHttps)
        {
            failures.Add($"SSO provider '{providerName}' must define an HTTPS Authority URL.");
        }

        if (string.IsNullOrWhiteSpace(provider.CallbackPath) || !provider.CallbackPath.StartsWith('/'))
        {
            failures.Add($"SSO provider '{providerName}' must define CallbackPath starting with '/'.");
        }

        if (!string.IsNullOrWhiteSpace(provider.RedirectUri))
        {
            if (!Uri.TryCreate(provider.RedirectUri, UriKind.Absolute, out var oauthRedirectUri))
            {
                failures.Add($"SSO provider '{providerName}' has invalid RedirectUri '{provider.RedirectUri}'.");
            }
            else
            {
                var isLocalhost = oauthRedirectUri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                    || oauthRedirectUri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);

                if (oauthRedirectUri.Scheme != Uri.UriSchemeHttps && !(environment.IsDevelopment() && isLocalhost))
                {
                    failures.Add($"SSO provider '{providerName}' RedirectUri '{provider.RedirectUri}' must use HTTPS outside local development.");
                }
            }
        }

        if (provider.RedirectUrls.Count == 0)
        {
            failures.Add($"SSO provider '{providerName}' must define at least one RedirectUrls entry.");
        }

        foreach (var redirectUrl in provider.RedirectUrls)
        {
            if (!Uri.TryCreate(redirectUrl, UriKind.Absolute, out var redirectUri))
            {
                failures.Add($"SSO provider '{providerName}' has invalid redirect URL '{redirectUrl}'.");
                continue;
            }

            var isLocalhost = redirectUri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                || redirectUri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);

            if (redirectUri.Scheme != Uri.UriSchemeHttps && !(environment.IsDevelopment() && isLocalhost))
            {
                failures.Add($"SSO provider '{providerName}' redirect URL '{redirectUrl}' must use HTTPS outside local development.");
            }

            // Detect collision: a frontend RedirectUrls entry that exactly matches the OAuth
            // RedirectUri causes a redirect loop (backend bounces browser to its own API
            // callback, which crashes because it expects ?code/state, not ?sessionCode).
            if (!string.IsNullOrWhiteSpace(provider.RedirectUri)
                && string.Equals(redirectUrl.TrimEnd('/'), provider.RedirectUri.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
            {
                failures.Add(
                    $"SSO provider '{providerName}' has a RedirectUrls entry '{redirectUrl}' that is identical to RedirectUri. " +
                    "RedirectUrls must list FRONTEND URLs (e.g. /auth/callback/google), not the OAuth API callback URL.");
            }
        }
    }

    private static bool IsInvalidSecret(string secret)
    {
        return string.IsNullOrWhiteSpace(secret) || SecretPlaceholders.Contains(secret.Trim());
    }
}
