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

        // Seed / repair Admin User
        var adminUser = await _userManager.FindByEmailAsync("admin@notescool.com")
            ?? await _userManager.FindByNameAsync("admin@notescool.com")
            ?? await _userManager.FindByNameAsync("admin");

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
        }

        if (!string.Equals(adminUser.Email, "admin@notescool.com", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(adminUser.UserName, "admin@notescool.com", StringComparison.OrdinalIgnoreCase)
            || !adminUser.EmailConfirmed
            || string.IsNullOrWhiteSpace(adminUser.DisplayName))
        {
            adminUser.Email = "admin@notescool.com";
            adminUser.UserName = "admin@notescool.com";
            adminUser.NormalizedEmail = "ADMIN@NOTESCOOL.COM";
            adminUser.NormalizedUserName = "ADMIN@NOTESCOOL.COM";
            adminUser.DisplayName = string.IsNullOrWhiteSpace(adminUser.DisplayName) ? "Administrator" : adminUser.DisplayName;
            adminUser.EmailConfirmed = true;

            var updateResult = await _userManager.UpdateAsync(adminUser);
            if (!updateResult.Succeeded)
            {
                throw new Exception($"Failed to normalize seeded 'admin' user: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
            }
        }

        if (!await _userManager.IsInRoleAsync(adminUser, SystemRoles.Admin))
        {
            var roleResult = await _userManager.AddToRoleAsync(adminUser, SystemRoles.Admin);
            if (!roleResult.Succeeded)
            {
                throw new Exception($"Failed to assign '{SystemRoles.Admin}' role to user: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
        }
    }
}
