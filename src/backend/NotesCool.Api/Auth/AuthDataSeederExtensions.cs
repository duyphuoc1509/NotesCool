using Microsoft.EntityFrameworkCore;
using NotesCool.Api.Auth;

namespace NotesCool.Api.Extensions;

public static class AuthDataSeederExtensions
{
    public static async Task SeedAuthDataAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AuthDataSeeder");

        try
        {
            var adminEmail = "admin@notescool.local";
            var exists = await dbContext.Users.AnyAsync(u => u.Email == adminEmail || u.Email == "admin");
            
            if (!exists)
            {
                var admin = new UserAccount
                {
                    Email = adminEmail,
                    PasswordHash = PasswordHasher.Hash("Admin@123"),
                    Status = UserAccountStatuses.Active,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                dbContext.Users.Add(admin);
                
                // Also add shorthand "admin" username if system allows it as email string
                var adminShort = new UserAccount
                {
                    Email = "admin",
                    PasswordHash = PasswordHasher.Hash("Admin@123"),
                    Status = UserAccountStatuses.Active,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                dbContext.Users.Add(adminShort);

                await dbContext.SaveChangesAsync();
                logger.LogInformation("Admin user seeded to AuthDbContext.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding AuthDbContext.");
        }
    }
}
