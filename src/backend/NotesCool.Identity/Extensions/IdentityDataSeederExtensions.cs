using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotesCool.Identity.Infrastructure;
using NotesCool.Shared.Auth;

namespace NotesCool.Identity.Extensions;

/// <summary>
/// Optional standalone seed entry point — aligned with <see cref="IdentityDataSeeder"/> canonical admin.
/// </summary>
public static class IdentityDataSeederExtensions
{
    public static async Task SeedIdentityDataAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IdentityDataSeeder>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("IdentityDataSeederExtensions");

        try
        {
            await seeder.SeedAsync();
            logger.LogInformation(
                "Identity seed completed (admin: {Email}).",
                IdentityDataSeeder.SeededAdminEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding identity data.");
            throw;
        }
    }
}
