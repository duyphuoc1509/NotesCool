using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NotesCool.Api.Auth;
using Xunit;

namespace NotesCool.Notes.Tests.Api;

public class RegistrationEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public RegistrationEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_WithValidRequest_CreatesActiveAccount()
    {
        using var client = CreateClient(nameof(Register_WithValidRequest_CreatesActiveAccount));
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("new.user@example.com", "StrongPass1!"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        body.Should().NotBeNull();
        body!.Email.Should().Be("new.user@example.com");
        body.Status.Should().Be("active");
        body.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_StoresHashedPassword()
    {
        var databaseName = nameof(Register_StoresHashedPassword);
        using var client = CreateClient(databaseName);
        var request = new RegisterRequest("hashcheck@example.com", "StrongPass1!");

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var scope = CreateFactory(databaseName).Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var storedUser = await db.Users.SingleAsync(u => u.Email == request.Email);
        storedUser.PasswordHash.Should().NotBe(request.Password);
        storedUser.PasswordHash.Should().StartWith("PBKDF2-SHA256$");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsClearConflictError()
    {
        using var client = CreateClient(nameof(Register_WithDuplicateEmail_ReturnsClearConflictError));
        var request = new RegisterRequest("duplicate@example.com", "StrongPass1!");

        (await client.PostAsJsonAsync("/api/auth/register", request)).StatusCode.Should().Be(HttpStatusCode.Created);
        var duplicate = await client.PostAsJsonAsync("/api/auth/register", request);

        duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var error = await duplicate.Content.ReadAsStringAsync();
        error.Should().Contain("Email already exists");
    }

    [Theory]
    [InlineData("short1!")]
    [InlineData("alllowercase1!")]
    [InlineData("ALLUPPERCASE1!")]
    [InlineData("NoNumber!")]
    [InlineData("NoSpecial1")]
    public async Task Register_WithWeakPassword_ReturnsValidationError(string password)
    {
        using var client = CreateClient($"weak-{password}");
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("weak@example.com", password));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadAsStringAsync();
        error.Should().Contain("Password");
    }

    private HttpClient CreateClient(string databaseName) => CreateFactory(databaseName).CreateClient();

    private WebApplicationFactory<Program> CreateFactory(string databaseName)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AuthDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AuthDbContext>(options => options.UseInMemoryDatabase(databaseName));
            });
        });
    }
}
