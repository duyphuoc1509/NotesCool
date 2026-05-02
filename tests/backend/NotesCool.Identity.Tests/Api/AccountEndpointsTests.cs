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
using NotesCool.Shared.Auth;
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
        payload.User.Roles.Should().Contain(SystemRoles.User);
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
    public async Task Login_WithValidCredentials_ReturnsJwtContainingSubjectEmailAndRole()
    {
        using var client = CreateClient();
        await client.PostAsJsonAsync("/api/account/register", new RegisterRequest("login@example.com", "Password123!", "Login User"));

        var response = await client.PostAsJsonAsync("/api/account/login", new LoginRequest("login@example.com", "Password123!"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<AuthResponse>();
        payload.Should().NotBeNull();
        payload!.User.Roles.Should().Contain(SystemRoles.User);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(payload.AccessToken);
        token.Subject.Should().NotBeNullOrWhiteSpace();
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "login@example.com");
        token.Claims.Should().Contain(c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == SystemRoles.User);
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

    [Fact]
    public async Task AdminUsersEndpoint_WithoutAuth_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/admin/users");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminUsersEndpoint_WithNonAdminRole_ReturnsForbidden()
    {
        using var client = CreateClient();
        var token = await RegisterAndLoginAsync(client, "user@example.com", "Password123!", "User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/admin/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminUsersEndpoint_WithAdminRole_ReturnsUsers()
    {
        using var client = CreateClient();
        await client.PostAsJsonAsync("/api/account/register", new RegisterRequest("member@example.com", "Password123!", "Member"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsSeededAdminAsync(client));

        var response = await client.GetAsync("/api/admin/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<List<UserManagementResponse>>();
        payload.Should().NotBeNull();
        payload!.Should().Contain(x => x.Email == "admin@notescool.com" && x.Roles.Contains(SystemRoles.Admin));
        payload.Should().Contain(x => x.Email == "member@example.com" && x.Roles.Contains(SystemRoles.User));
    }

    [Fact]
    public async Task AdminCanBlockNormalUser()
    {
        var app = CreateApp($"IdentityDb-{Guid.NewGuid()}", $"NotesDb-{Guid.NewGuid()}", $"TasksDb-{Guid.NewGuid()}");
        using var client = app.CreateClient();
        await client.PostAsJsonAsync("/api/account/register", new RegisterRequest("blockme@example.com", "Password123!", "Block Me"));

        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var targetUser = await dbContext.Users.SingleAsync(x => x.Email == "blockme@example.com");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsSeededAdminAsync(client));

        var response = await client.PutAsJsonAsync($"/api/admin/users/{targetUser.Id}/status", new UpdateUserStatusRequest("Suspended"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<UserManagementResponse>();
        payload.Should().NotBeNull();
        payload!.Status.Should().Be("Suspended");

        dbContext.Entry(targetUser).State = EntityState.Detached;
        var updatedUser = await dbContext.Users.SingleAsync(x => x.Id == targetUser.Id);
        updatedUser.Status.Should().Be(AccountStatus.Suspended);
    }

    [Fact]
    public async Task BlockedUser_CannotLogin()
    {
        var app = CreateApp($"IdentityDb-{Guid.NewGuid()}", $"NotesDb-{Guid.NewGuid()}", $"TasksDb-{Guid.NewGuid()}");
        using var client = app.CreateClient();
        await client.PostAsJsonAsync("/api/account/register", new RegisterRequest("blocked-login@example.com", "Password123!", "Blocked Login"));

        await using (var scope = app.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            var user = await dbContext.Users.SingleAsync(x => x.Email == "blocked-login@example.com");
            user.Status = AccountStatus.Suspended;
            await dbContext.SaveChangesAsync();
        }

        var response = await client.PostAsJsonAsync("/api/account/login", new LoginRequest("blocked-login@example.com", "Password123!"));

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

    private static async Task<string> RegisterAndLoginAsync(HttpClient client, string email, string password, string displayName)
    {
        await client.PostAsJsonAsync("/api/account/register", new RegisterRequest(email, password, displayName));
        var loginResponse = await client.PostAsJsonAsync("/api/account/login", new LoginRequest(email, password));
        var payload = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        return payload!.AccessToken;
    }

    private static async Task<string> LoginAsSeededAdminAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/account/login", new LoginRequest("admin@notescool.com", "P@ssword123!"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<AuthResponse>();
        payload.Should().NotBeNull();
        payload!.User.Roles.Should().Contain(SystemRoles.Admin);
        return payload.AccessToken;
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
