namespace NotesCool.Api.Configuration;

public interface ISsoConfigService
{
    SsoOptions GetOptions();
}

public sealed class SsoEnvironmentConfigService(ILogger<SsoEnvironmentConfigService> logger) : ISsoConfigService
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
                    "SSO_GOOGLE",
                    "https://accounts.google.com",
                    "/signin-google",
                    "https://localhost:10001/auth/callback/google"),
                BuildProvider(
                    MicrosoftProviderName,
                    "SSO_MICROSOFT",
                    "https://login.microsoftonline.com/common/v2.0",
                    "/signin-microsoft",
                    "https://localhost:10001/auth/callback/microsoft")
            ]
        };
    }

    private SsoProviderOptions BuildProvider(
        string providerName,
        string environmentPrefix,
        string defaultAuthority,
        string defaultCallbackPath,
        string defaultRedirectUrl)
    {
        var enabled = ReadBoolean($"{environmentPrefix}_ENABLED");
        var clientId = ReadString($"{environmentPrefix}_CLIENT_ID");
        var clientSecret = ReadString($"{environmentPrefix}_CLIENT_SECRET");
        var authority = defaultAuthority; // Prevent users from mistakenly overriding Google/MS authority with their app domain
        var callbackPath = ReadString($"{environmentPrefix}_CALLBACK_PATH", defaultCallbackPath);
        var redirectUrls = ReadList($"{environmentPrefix}_REDIRECT_URLS", defaultRedirectUrl);

        LogMissingConfiguration(providerName, environmentPrefix, enabled, clientId, clientSecret, authority, callbackPath, redirectUrls);

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

    private static bool ReadBoolean(string variableName)
    {
        var rawValue = Environment.GetEnvironmentVariable(variableName);
        return bool.TryParse(rawValue?.Trim(), out var enabled) && enabled;
    }

    private static string ReadString(string variableName, string defaultValue = "")
    {
        var value = Environment.GetEnvironmentVariable(variableName);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
    }

    private static List<string> ReadList(string variableName, string defaultValue)
    {
        var rawValue = Environment.GetEnvironmentVariable(variableName);
        var value = string.IsNullOrWhiteSpace(rawValue) ? defaultValue : rawValue;

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToList();
    }
}
