using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotesCool.Identity.Infrastructure;

namespace NotesCool.Identity.Extensions;

public static class IdentityDataSeederExtensions
{
    public static async Task SeedIdentityDataAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("IdentityDataSeeder");

        try
        {
            // Create "admin" role if not exists
            if (!await roleManager.RoleExistsAsync("admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("admin"));
                logger.LogInformation("Role 'admin' created successfully.");
            }

            // Create admin user if not exists
            var adminEmail = "admin";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    DisplayName = "System Admin",
                    EmailConfirmed = true,
                    Status = AccountStatus.Active
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    logger.LogInformation("Admin user created successfully.");
                }
                else
                {
                    logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            // Assign role
            if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "admin"))
            {
                var result = await userManager.AddToRoleAsync(adminUser, "admin");
                if (result.Succeeded)
                {
                    logger.LogInformation("Role 'admin' assigned to admin user.");
                }
                else
                {
                    logger.LogError("Failed to assign role 'admin' to admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
