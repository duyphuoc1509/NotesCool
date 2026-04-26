using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NotesCool.Api.Auth;
using Xunit;

namespace NotesCool.Notes.Tests.Api;

public class AuthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Refresh_WithValidRefreshToken_ReturnsNewTokens()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("user@example.com", "password123"));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginTokens = await loginResponse.Content.ReadFromJsonAsync<AuthTokenResponse>();

        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(loginTokens!.RefreshToken));

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshedTokens = await refreshResponse.Content.ReadFromJsonAsync<AuthTokenResponse>();
        refreshedTokens.Should().NotBeNull();
        refreshedTokens!.AccessToken.Should().NotBeNullOrWhiteSpace();
        refreshedTokens.RefreshToken.Should().NotBeNullOrWhiteSpace();
        refreshedTokens.RefreshToken.Should().NotBe(loginTokens.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WithReusedRefreshToken_ReturnsUnauthorized()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("user@example.com", "password123"));
        var loginTokens = await loginResponse.Content.ReadFromJsonAsync<AuthTokenResponse>();

        var firstRefresh = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(loginTokens!.RefreshToken));
        firstRefresh.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondRefresh = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(loginTokens.RefreshToken));

        secondRefresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_RevokesRefreshToken_AndRefreshReturnsUnauthorized()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("user@example.com", "password123"));
        var loginTokens = await loginResponse.Content.ReadFromJsonAsync<AuthTokenResponse>();

        var logoutResponse = await _client.PostAsJsonAsync("/api/auth/logout", new RefreshTokenRequest(loginTokens!.RefreshToken));
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(loginTokens.RefreshToken));

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithExpiredRefreshToken_ReturnsUnauthorized()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("user@example.com", "password123", -1));
        var loginTokens = await loginResponse.Content.ReadFromJsonAsync<AuthTokenResponse>();

        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(loginTokens!.RefreshToken));

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

}
