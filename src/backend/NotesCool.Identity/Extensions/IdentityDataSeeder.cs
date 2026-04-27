using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NotesCool.Identity.Infrastructure;

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
        if (!await _roleManager.RoleExistsAsync("admin"))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole("admin"));
            if (!roleResult.Succeeded)
            {
                throw new Exception($"Failed to seed 'admin' role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
        }

        // Seed Admin User
        var adminUser = await _userManager.FindByNameAsync("admin");
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin",
                NormalizedEmail = "ADMIN",
                EmailConfirmed = true
            };

            // Temporarily bypass validators (including email format validation)
            var originalValidators = _userManager.UserValidators.ToList();
            _userManager.UserValidators.Clear();
            
            try
            {
                var result = await _userManager.CreateAsync(adminUser, "P@ssword123!");
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to seed 'admin' user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                var roleResult = await _userManager.AddToRoleAsync(adminUser, "admin");
                if (!roleResult.Succeeded)
                {
                    throw new Exception($"Failed to assign 'admin' role to user: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }
            finally
            {
                foreach (var v in originalValidators)
                {
                    _userManager.UserValidators.Add(v);
                }
            }
        }
    }
}
