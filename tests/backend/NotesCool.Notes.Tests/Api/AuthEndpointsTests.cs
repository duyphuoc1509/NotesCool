using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NotesCool.Api.Auth;
using Xunit;

namespace NotesCool.Notes.Tests.Api;

public class AuthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IUserCredentialStore>(new InMemoryUserCredentialStore(new[]
                {
                    new RegisteredUser("user-1", "jane@example.com", "Jane Doe", "P@ssw0rd!")
                }));
            });
        }).CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAccessTokenAndUserInfo()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("jane@example.com", "P@ssw0rd!"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();

        login.Should().NotBeNull();
        login!.AccessToken.Should().NotBeNullOrWhiteSpace();
        login.TokenType.Should().Be("Bearer");
        login.ExpiresIn.Should().BeGreaterThan(0);
        login.User.Id.Should().Be("user-1");
        login.User.Email.Should().Be("jane@example.com");
        login.User.DisplayName.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsGenericUnauthorizedWithoutLeakingAccountExistence()
    {
        var wrongPassword = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("jane@example.com", "wrong"));
        var unknownEmail = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("missing@example.com", "wrong"));

        wrongPassword.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        unknownEmail.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var wrongPasswordBody = await wrongPassword.Content.ReadAsStringAsync();
        var unknownEmailBody = await unknownEmail.Content.ReadAsStringAsync();
        wrongPasswordBody.Should().Be(unknownEmailBody);
        wrongPasswordBody.Should().NotContain("jane@example.com");
        wrongPasswordBody.Should().NotContain("missing@example.com");
    }
}
