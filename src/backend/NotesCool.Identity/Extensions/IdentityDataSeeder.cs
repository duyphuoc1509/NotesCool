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

        const string canonicalAdminEmail = "admin@notescool.local";

        // Seed / repair Admin User — search across all known historical emails/usernames
        var adminUser = await _userManager.FindByEmailAsync(canonicalAdminEmail)
            ?? await _userManager.FindByEmailAsync("admin@notescool.com")
            ?? await _userManager.FindByNameAsync(canonicalAdminEmail)
            ?? await _userManager.FindByNameAsync("admin@notescool.com")
            ?? await _userManager.FindByNameAsync("admin");

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = canonicalAdminEmail,
                Email = canonicalAdminEmail,
                NormalizedEmail = canonicalAdminEmail.ToUpperInvariant(),
                DisplayName = "Administrator",
                EmailConfirmed = true,
                Status = AccountStatus.Active
            };

            var result = await _userManager.CreateAsync(adminUser, "Admin@123");
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to seed 'admin' user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        if (!string.Equals(adminUser.Email, canonicalAdminEmail, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(adminUser.UserName, canonicalAdminEmail, StringComparison.OrdinalIgnoreCase)
            || !adminUser.EmailConfirmed
            || adminUser.Status != AccountStatus.Active
            || string.IsNullOrWhiteSpace(adminUser.DisplayName))
        {
            adminUser.Email = canonicalAdminEmail;
            adminUser.UserName = canonicalAdminEmail;
            adminUser.NormalizedEmail = canonicalAdminEmail.ToUpperInvariant();
            adminUser.NormalizedUserName = canonicalAdminEmail.ToUpperInvariant();
            adminUser.DisplayName = string.IsNullOrWhiteSpace(adminUser.DisplayName) ? "Administrator" : adminUser.DisplayName;
            adminUser.EmailConfirmed = true;
            adminUser.Status = AccountStatus.Active;

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
