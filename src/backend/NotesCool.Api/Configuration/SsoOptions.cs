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
        }
    }

    private static bool IsInvalidSecret(string secret)
    {
        return string.IsNullOrWhiteSpace(secret) || SecretPlaceholders.Contains(secret.Trim());
    }
}
