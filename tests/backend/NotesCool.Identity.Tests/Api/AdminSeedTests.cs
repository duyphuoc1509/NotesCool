using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NotesCool.Identity.Extensions;
using NotesCool.Identity.Infrastructure;
using NotesCool.Shared.Auth;
using Xunit;

namespace NotesCool.Identity.Tests.Api;

public class AdminSeedTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AdminSeedTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Seed_ShouldCreateAdminUserAndRole()
    {
        var app = CreateApp($"IdentityDb-{Guid.NewGuid()}");

        await using var scope = app.Services.CreateAsyncScope();
        var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
        roleManager.Should().NotBeNull("RoleManager should be registered");

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var roleExists = await roleManager!.RoleExistsAsync(SystemRoles.Admin);
        roleExists.Should().BeTrue("admin role should be seeded");

        var user = await userManager.FindByNameAsync("admin@notescool.com");
        user.Should().NotBeNull("admin user should be seeded");
        user!.Email.Should().Be("admin@notescool.com");

        var isInRole = await userManager.IsInRoleAsync(user, SystemRoles.Admin);
        isInRole.Should().BeTrue("admin user should be in admin role");
    }

    [Fact]
    public async Task Seed_ShouldRepairLegacyAdminUserAndAssignAdminRole()
    {
        var app = CreateApp($"IdentityDb-{Guid.NewGuid()}");

        await using var scope = app.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var seeder = scope.ServiceProvider.GetRequiredService<IdentityDataSeeder>();

        var adminUser = await userManager.FindByEmailAsync("admin@notescool.com");
        adminUser.Should().NotBeNull();

        adminUser!.UserName = "admin";
        adminUser.EmailConfirmed = false;
        adminUser.DisplayName = string.Empty;

        var removeRoleResult = await userManager.RemoveFromRoleAsync(adminUser, SystemRoles.Admin);
        removeRoleResult.Succeeded.Should().BeTrue();

        var updateResult = await userManager.UpdateAsync(adminUser);
        updateResult.Succeeded.Should().BeTrue();

        await seeder.SeedAsync();

        var repairedAdmin = await userManager.FindByEmailAsync("admin@notescool.com");
        repairedAdmin.Should().NotBeNull();
        repairedAdmin!.UserName.Should().Be("admin@notescool.com");
        repairedAdmin.EmailConfirmed.Should().BeTrue();
        repairedAdmin.DisplayName.Should().Be("Administrator");
        (await userManager.IsInRoleAsync(repairedAdmin, SystemRoles.Admin)).Should().BeTrue();
    }

    private WebApplicationFactory<Program> CreateApp(string identityDbName)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                ReplaceDbContext<IdentityDbContext>(services, identityDbName);
                ReplaceDbContext<NotesCool.Notes.Infrastructure.NotesDbContext>(services, $"NotesDb-{Guid.NewGuid()}");
                ReplaceDbContext<NotesCool.Tasks.Infrastructure.TasksDbContext>(services, $"TasksDb-{Guid.NewGuid()}");
            });
        });
    }

    private static void ReplaceDbContext<TContext>(IServiceCollection services, string dbName)
        where TContext : DbContext
    {
        var optionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TContext>));
        if (optionsDescriptor is not null)
        {
            services.Remove(optionsDescriptor);
        }

        var contextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TContext));
        if (contextDescriptor is not null)
        {
            services.Remove(contextDescriptor);
        }

        services.AddDbContext<TContext>(options => options.UseInMemoryDatabase(dbName));
    }
}
