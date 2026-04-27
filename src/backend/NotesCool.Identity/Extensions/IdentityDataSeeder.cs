using Microsoft.AspNetCore.Identity;
using NotesCool.Identity.Infrastructure;

namespace NotesCool.Identity.Extensions;

public class IdentityDataSeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public IdentityDataSeeder(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
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
