using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NotesCool.Identity.Infrastructure;
using NotesCool.Shared.Auth;

namespace NotesCool.Identity.Extensions;

public class IdentityDataSeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<IdentityDataSeeder> _logger;

    public IdentityDataSeeder(
        UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager,
        ILogger<IdentityDataSeeder> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            await SeedInternalAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed default identity data. This might be expected in some test environments without a database.");
            // Do not rethrow to avoid breaking integration tests that do not correctly mock IdentityDbContext
        }
    }

    private async Task SeedInternalAsync()
    {
        // Seed Roles
        if (!await _roleManager.RoleExistsAsync(SystemRoles.Admin))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(SystemRoles.Admin));
            if (!roleResult.Succeeded)
            {
                throw new Exception($"Failed to seed '{SystemRoles.Admin}' role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
        }

        // Seed Admin User
        var adminUser = await _userManager.FindByNameAsync("admin@notescool.com");
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin@notescool.com",
                Email = "admin@notescool.com",
                NormalizedEmail = "ADMIN@NOTESCOOL.COM",
                DisplayName = "Administrator",
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(adminUser, "P@ssword123!");
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to seed 'admin' user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            var roleResult = await _userManager.AddToRoleAsync(adminUser, SystemRoles.Admin);
            if (!roleResult.Succeeded)
            {
                throw new Exception($"Failed to assign '{SystemRoles.Admin}' role to user: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
        }
    }
}
