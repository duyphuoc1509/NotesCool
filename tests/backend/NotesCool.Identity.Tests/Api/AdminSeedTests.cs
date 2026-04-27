using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NotesCool.Identity.Infrastructure;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

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
        
        using var scope = app.Services.CreateAsyncScope();
        
        // We expect RoleManager to be available after we fix the implementation
        var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
        roleManager.Should().NotBeNull("RoleManager should be registered");

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Verify Role admin exists
        var roleExists = await roleManager!.RoleExistsAsync("admin");
        roleExists.Should().BeTrue("admin role should be seeded");

        // Verify User admin exists
        var user = await userManager.FindByNameAsync("admin");
        user.Should().NotBeNull("admin user should be seeded");
        user!.Email.Should().Be("admin");
        
        // Verify User is in Role admin
        var isInRole = await userManager.IsInRoleAsync(user, "admin");
        isInRole.Should().BeTrue("admin user should be in admin role");
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
