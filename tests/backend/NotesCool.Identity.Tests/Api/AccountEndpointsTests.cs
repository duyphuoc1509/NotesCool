using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NotesCool.Identity.Contracts;
using NotesCool.Identity.Infrastructure;
using Xunit;

namespace NotesCool.Identity.Tests.Api;

public class AccountEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AccountEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_ReturnsCreated_WithUserProfile()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/account/register", new RegisterRequest(
            "boss@example.com",
            "Password123!",
            "Boss P"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadFromJsonAsync<AuthResponse>();
        payload.Should().NotBeNull();
        payload!.User.Email.Should().Be("boss@example.com");
        payload.User.DisplayName.Should().Be("Boss P");
        payload.User.Status.Should().Be("Active");
        payload.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        using var client = CreateClient();
        var request = new RegisterRequest("duplicate@example.com", "Password123!", "Duplicate User");

        var firstResponse = await client.PostAsJsonAsync("/api/account/register", request);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var secondResponse = await client.PostAsJsonAsync("/api/account/register", request);

        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsJwtContainingSubjectAndEmail()
    {
        using var client = CreateClient();
        await client.PostAsJsonAsync("/api/account/register", new RegisterRequest("login@example.com", "Password123!", "Login User"));

        var response = await client.PostAsJsonAsync("/api/account/login", new LoginRequest("login@example.com", "Password123!"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<AuthResponse>();
        payload.Should().NotBeNull();
        var token = new JwtSecurityTokenHandler().ReadJwtToken(payload!.AccessToken);
        token.Subject.Should().NotBeNullOrWhiteSpace();
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "login@example.com");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        using var client = CreateClient();
        await client.PostAsJsonAsync("/api/account/register", new RegisterRequest("wrong-pass@example.com", "Password123!", "Wrong Pass"));

        var response = await client.PostAsJsonAsync("/api/account/login", new LoginRequest("wrong-pass@example.com", "WrongPassword123!"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithBearerToken_ReturnsNoContent()
    {
        using var client = CreateClient();
        await client.PostAsJsonAsync("/api/account/register", new RegisterRequest("logout@example.com", "Password123!", "Logout User"));
        var loginResponse = await client.PostAsJsonAsync("/api/account/login", new LoginRequest("logout@example.com", "Password123!"));
        var payload = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", payload!.AccessToken);

        var response = await client.PostAsync("/api/account/logout", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Login_ForInactiveAccount_ReturnsForbidden()
    {
        var app = CreateApp($"IdentityDb-{Guid.NewGuid()}", $"NotesDb-{Guid.NewGuid()}", $"TasksDb-{Guid.NewGuid()}");
        using var client = app.CreateClient();
        await client.PostAsJsonAsync("/api/account/register", new RegisterRequest("inactive@example.com", "Password123!", "Inactive User"));

        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var user = await dbContext.Users.SingleAsync(x => x.Email == "inactive@example.com");
        user.Status = AccountStatus.Inactive;
        await dbContext.SaveChangesAsync();

        var response = await client.PostAsJsonAsync("/api/account/login", new LoginRequest("inactive@example.com", "Password123!"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private HttpClient CreateClient()
    {
        return CreateApp($"IdentityDb-{Guid.NewGuid()}", $"NotesDb-{Guid.NewGuid()}", $"TasksDb-{Guid.NewGuid()}").CreateClient();
    }

    private WebApplicationFactory<Program> CreateApp(string identityDbName, string notesDbName, string tasksDbName)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                ReplaceDbContext<IdentityDbContext>(services, identityDbName);
                ReplaceDbContext<NotesCool.Notes.Infrastructure.NotesDbContext>(services, notesDbName);
                ReplaceDbContext<NotesCool.Tasks.Infrastructure.TasksDbContext>(services, tasksDbName);
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
