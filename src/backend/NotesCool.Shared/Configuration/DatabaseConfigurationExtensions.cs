using Microsoft.Extensions.Configuration;

namespace NotesCool.Shared.Configuration;

public static class DatabaseConfigurationExtensions
{
    private const string DefaultConnectionName = "DefaultConnection";

    /// <summary>
    /// Resolves the default database connection string.
    /// When the five individual Database:Internal:* settings (or their
    /// corresponding DATABASE_INTERNAL_* environment variables) are all
    /// present, the connection string is built from those components.
    /// Otherwise the classic ConnectionStrings:DefaultConnection value is
    /// returned unchanged.
    /// </summary>
    public static string GetDefaultConnectionString(this IConfiguration configuration)
    {
        var configuredConnectionString = configuration.GetConnectionString(DefaultConnectionName);

        var host = Resolve(configuration, "Database:Internal:Host", "DATABASE_INTERNAL_HOST");
        var port = Resolve(configuration, "Database:Internal:Port", "DATABASE_INTERNAL_PORT");
        var database = Resolve(configuration, "Database:Internal:Name", "DATABASE_INTERNAL_NAME");
        var username = Resolve(configuration, "Database:Internal:User", "DATABASE_INTERNAL_USER");
        var password = Resolve(configuration, "Database:Internal:Password", "DATABASE_INTERNAL_PASSWORD");

        if (string.IsNullOrWhiteSpace(host)
            || string.IsNullOrWhiteSpace(port)
            || string.IsNullOrWhiteSpace(database)
            || string.IsNullOrWhiteSpace(username)
            || string.IsNullOrWhiteSpace(password))
        {
            return configuredConnectionString
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' is not configured and the Database:Internal:* " +
                    "settings are incomplete. Provide either a full connection string or all five " +
                    "DATABASE_INTERNAL_HOST, DATABASE_INTERNAL_PORT, DATABASE_INTERNAL_NAME, " +
                    "DATABASE_INTERNAL_USER, and DATABASE_INTERNAL_PASSWORD values.");
        }

        if (!int.TryParse(port, out _))
        {
            throw new InvalidOperationException(
                $"DATABASE_INTERNAL_PORT value '{port}' is not a valid integer.");
        }

        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }

    private static string? Resolve(IConfiguration configuration, string configKey, string envVarName)
    {
        var value = configuration[configKey];
        if (!string.IsNullOrWhiteSpace(value))
            return value;

        return Environment.GetEnvironmentVariable(envVarName);
    }
}
