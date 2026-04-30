namespace NotesCool.Api.Configuration;

public interface ISsoConfigService
{
    SsoOptions GetOptions();
}

public sealed class SsoEnvironmentConfigService(ILogger<SsoEnvironmentConfigService> logger, IConfiguration configuration) : ISsoConfigService
{
    private const string GoogleProviderName = "Google";
    private const string MicrosoftProviderName = "Microsoft";

    public SsoOptions GetOptions()
    {
        return new SsoOptions
        {
            Providers =
            [
                BuildProvider(
                    GoogleProviderName,
                    ["SSO_GOOGLE", "GOOGLE_SSO"],
                    "Sso:Google",
                    "https://accounts.google.com",
                    "/signin-google",
                    "https://localhost:10001/auth/callback/google"),
                BuildProvider(
                    MicrosoftProviderName,
                    ["SSO_MICROSOFT", "MICROSOFT_SSO"],
                    "Sso:Microsoft",
                    "https://login.microsoftonline.com/common/v2.0",
                    "/signin-microsoft",
                    "https://localhost:10001/auth/callback/microsoft")
            ]
        };
    }

    private SsoProviderOptions BuildProvider(
        string providerName,
        string[] environmentPrefixes,
        string configurationSection,
        string defaultAuthority,
        string defaultCallbackPath,
        string defaultRedirectUrl)
    {
        var enabled = ReadBoolean(environmentPrefixes.Select(p => $"{p}_ENABLED").ToArray(), $"{configurationSection}:Enabled");
        var clientId = ReadString(environmentPrefixes.Select(p => $"{p}_CLIENT_ID").ToArray(), $"{configurationSection}:ClientId");
        var clientSecret = ReadString(environmentPrefixes.Select(p => $"{p}_CLIENT_SECRET").ToArray(), $"{configurationSection}:ClientSecret");
        var authority = ReadString(environmentPrefixes.Select(p => $"{p}_AUTHORITY").ToArray(), $"{configurationSection}:Authority", defaultAuthority);
        var callbackPath = ReadString(environmentPrefixes.Select(p => $"{p}_CALLBACK_PATH").ToArray(), $"{configurationSection}:CallbackPath", defaultCallbackPath);
        var redirectUrls = ReadList(environmentPrefixes.Select(p => $"{p}_REDIRECT_URLS").ToArray(), $"{configurationSection}:RedirectUrls", defaultRedirectUrl);

        var primaryPrefix = environmentPrefixes[0];
        LogMissingConfiguration(providerName, primaryPrefix, enabled, clientId, clientSecret, authority, callbackPath, redirectUrls);

        return new SsoProviderOptions
        {
            Name = providerName,
            Enabled = enabled,
            ClientId = clientId,
            ClientSecret = clientSecret,
            Authority = authority,
            CallbackPath = callbackPath,
            RedirectUrls = redirectUrls
        };
    }

    private void LogMissingConfiguration(
        string providerName,
        string environmentPrefix,
        bool enabled,
        string clientId,
        string clientSecret,
        string authority,
        string callbackPath,
        IReadOnlyCollection<string> redirectUrls)
    {
        if (!enabled)
        {
            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            {
                logger.LogWarning(
                    "{ProviderName} SSO is disabled and missing client configuration. Set {ClientIdVariable} and {ClientSecretVariable} before enabling it.",
                    providerName,
                    $"{environmentPrefix}_CLIENT_ID",
                    $"{environmentPrefix}_CLIENT_SECRET");
            }

            return;
        }

        WarnIfMissing(providerName, $"{environmentPrefix}_CLIENT_ID", clientId);
        WarnIfMissing(providerName, $"{environmentPrefix}_CLIENT_SECRET", clientSecret);
        WarnIfMissing(providerName, $"{environmentPrefix}_AUTHORITY", authority);
        WarnIfMissing(providerName, $"{environmentPrefix}_CALLBACK_PATH", callbackPath);

        if (redirectUrls.Count == 0)
        {
            logger.LogWarning(
                "{ProviderName} SSO is enabled but required environment variable {VariableName} is missing.",
                providerName,
                $"{environmentPrefix}_REDIRECT_URLS");
        }
    }

    private void WarnIfMissing(string providerName, string variableName, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        logger.LogWarning(
            "{ProviderName} SSO is enabled but required environment variable {VariableName} is missing.",
            providerName,
            variableName);
    }

    private bool ReadBoolean(string[] variableNames, string configurationKey)
    {
        var rawValue = variableNames
            .Select(Environment.GetEnvironmentVariable)
            .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            rawValue = configuration[configurationKey];
        }

        return bool.TryParse(rawValue, out var enabled) && enabled;
    }

    private string ReadString(string[] variableNames, string configurationKey, string defaultValue = "")
    {
        var value = variableNames
            .Select(Environment.GetEnvironmentVariable)
            .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

        if (string.IsNullOrWhiteSpace(value))
        {
            value = configuration[configurationKey];
        }

        return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
    }

    private List<string> ReadList(string[] variableNames, string configurationKey, string defaultValue)
    {
        var rawValue = variableNames
            .Select(Environment.GetEnvironmentVariable)
            .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            var configuredList = configuration.GetSection(configurationKey).Get<string[]>();
            if (configuredList is { Length: > 0 })
            {
                return configuredList
                    .Where(item => !string.IsNullOrWhiteSpace(item))
                    .Select(item => item.Trim())
                    .ToList();
            }

            rawValue = configuration[configurationKey];
        }

        var value = string.IsNullOrWhiteSpace(rawValue) ? defaultValue : rawValue;

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToList();
    }
}
