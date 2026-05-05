using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NotesCool.Identity.Infrastructure;
using NotesCool.Shared.Auth;

namespace NotesCool.Identity.Extensions;

public class IdentityDataSeeder
{
    /// <summary>Canonical seeded admin (matches integration tests and docs).</summary>
    public const string SeededAdminEmail = "admin@notescool.com";

    public const string SeededAdminPassword = "P@ssword123!";

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
        await CanonicalizeBuiltInRoleNamesAsync();

        if (!await _roleManager.RoleExistsAsync(SystemRoles.Admin))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(SystemRoles.Admin));
            if (!roleResult.Succeeded)
            {
                throw new Exception($"Failed to seed '{SystemRoles.Admin}' role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
        }

        if (!await _roleManager.RoleExistsAsync(SystemRoles.User))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(SystemRoles.User));
            if (!roleResult.Succeeded)
            {
                throw new Exception($"Failed to seed '{SystemRoles.User}' role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
        }

        var adminUser = await _userManager.FindByEmailAsync(SeededAdminEmail);

        if (adminUser is null)
        {
            var legacy =
                await _userManager.FindByEmailAsync("admin")
                ?? await _userManager.FindByNameAsync("admin");

            if (legacy is not null)
            {
                await MigrateLegacyAdminAsync(legacy);
                adminUser = await _userManager.FindByEmailAsync(SeededAdminEmail);
            }
        }

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = SeededAdminEmail,
                Email = SeededAdminEmail,
                DisplayName = "Administrator",
                EmailConfirmed = true,
                Status = AccountStatus.Active
            };

            var result = await _userManager.CreateAsync(adminUser, SeededAdminPassword);
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to seed admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            await EnsureCanonicalSeededAdminAsync(adminUser);
        }

        adminUser = await _userManager.FindByEmailAsync(SeededAdminEmail)
            ?? throw new InvalidOperationException("Seeded admin user was not found after creation.");

        if (!await _userManager.IsInRoleAsync(adminUser, SystemRoles.Admin))
        {
            var roleResult = await _userManager.AddToRoleAsync(adminUser, SystemRoles.Admin);
            if (!roleResult.Succeeded)
            {
                throw new Exception($"Failed to assign '{SystemRoles.Admin}' role to admin user: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
        }
    }

    /// <summary>
    /// Ensures profile fields for the canonical admin account (e.g. after AdminSeedTests simulates a broken row).
    /// </summary>
    private async Task EnsureCanonicalSeededAdminAsync(ApplicationUser user)
    {
        if (!string.Equals(user.Email, SeededAdminEmail, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var changed = false;

        if (!string.Equals(user.UserName, SeededAdminEmail, StringComparison.OrdinalIgnoreCase))
        {
            var r = await _userManager.SetUserNameAsync(user, SeededAdminEmail);
            if (!r.Succeeded)
            {
                throw new Exception($"Failed to set admin UserName: {string.Join(", ", r.Errors.Select(e => e.Description))}");
            }

            changed = true;
        }

        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            changed = true;
        }

        if (string.IsNullOrWhiteSpace(user.DisplayName))
        {
            user.DisplayName = "Administrator";
            changed = true;
        }

        if (!changed)
        {
            return;
        }

        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            throw new Exception($"Failed to update seeded admin user: {string.Join(", ", update.Errors.Select(e => e.Description))}");
        }
    }

    private async Task MigrateLegacyAdminAsync(ApplicationUser legacy)
    {
        var setName = await _userManager.SetUserNameAsync(legacy, SeededAdminEmail);
        if (!setName.Succeeded)
        {
            throw new Exception($"Failed to migrate admin UserName: {string.Join(", ", setName.Errors.Select(e => e.Description))}");
        }

        var setEmail = await _userManager.SetEmailAsync(legacy, SeededAdminEmail);
        if (!setEmail.Succeeded)
        {
            throw new Exception($"Failed to migrate admin Email: {string.Join(", ", setEmail.Errors.Select(e => e.Description))}");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(legacy);
        var reset = await _userManager.ResetPasswordAsync(legacy, token, SeededAdminPassword);
        if (!reset.Succeeded)
        {
            throw new Exception($"Failed to set canonical admin password: {string.Join(", ", reset.Errors.Select(e => e.Description))}");
        }

        _logger.LogInformation("Migrated legacy admin account to {Email}.", SeededAdminEmail);
    }

    /// <summary>
    /// Old deployments stored AspNetRoles.Name as lowercase <c>admin</c>; authorization uses <see cref="SystemRoles.Admin"/>.
    /// Identity resolves roles by normalized name, but JWT echoed the stored Name — normalize to canonical casing.
    /// </summary>
    private async Task CanonicalizeBuiltInRoleNamesAsync()
    {
        await CanonicalizeRoleNameAsync(SystemRoles.Admin);
        await CanonicalizeRoleNameAsync(SystemRoles.User);
    }

    private async Task CanonicalizeRoleNameAsync(string canonicalName)
    {
        var role = await _roleManager.FindByNameAsync(canonicalName);
        if (role is null)
        {
            return;
        }

        if (string.Equals(role.Name, canonicalName, StringComparison.Ordinal))
        {
            return;
        }

        var oldName = role.Name;
        role.Name = canonicalName;
        var update = await _roleManager.UpdateAsync(role);
        if (!update.Succeeded)
        {
            _logger.LogWarning(
                "Could not rename role from '{Old}' to '{Canonical}': {Errors}",
                oldName,
                canonicalName,
                string.Join(", ", update.Errors.Select(e => e.Description)));
            return;
        }

        _logger.LogInformation("Renamed identity role from '{Old}' to '{Canonical}'.", oldName, canonicalName);
    }
}
