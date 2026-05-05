using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NotesCool.Identity.Infrastructure;
using NotesCool.Shared.Auth;

namespace NotesCool.Notes.Tests.Api;

/// <summary>
/// Uses in-memory Identity storage and seeds users expected by <see cref="AuthEndpointsTests"/>.
/// </summary>
public sealed class AuthEndpointsWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestPassword = "Password123!";

    private static readonly string InMemoryName = "AuthEndpoints-" + Guid.NewGuid().ToString("N");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<IdentityDbContext>>();
            services.AddDbContext<IdentityDbContext>(options => options.UseInMemoryDatabase(InMemoryName));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        SeedTestUsersAsync(scope.ServiceProvider).GetAwaiter().GetResult();
        return host;
    }

    private static async Task SeedTestUsersAsync(IServiceProvider sp)
    {
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var email in new[] { "logout@example.com", "refresh@example.com" })
        {
            if (await userManager.FindByEmailAsync(email) is not null)
            {
                continue;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DisplayName = "Auth Endpoints Test User",
                Status = AccountStatus.Active,
                EmailConfirmed = true
            };

            var create = await userManager.CreateAsync(user, TestPassword);
            if (!create.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to seed {email}: {string.Join("; ", create.Errors.Select(e => e.Description))}");
            }

            var addRole = await userManager.AddToRoleAsync(user, SystemRoles.User);
            if (!addRole.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to add role for {email}: {string.Join("; ", addRole.Errors.Select(e => e.Description))}");
            }
        }
    }
}
