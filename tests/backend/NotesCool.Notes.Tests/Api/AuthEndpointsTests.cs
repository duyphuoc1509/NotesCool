using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace NotesCool.Notes.Tests.Api;

public class AuthEndpointsTests : IClassFixture<AuthEndpointsWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(AuthEndpointsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Logout_InvalidatesCurrentRefreshToken_AndSubsequentRefreshFails()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("logout@example.com", AuthEndpointsWebApplicationFactory.TestPassword));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var tokens = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        tokens.Should().NotBeNull();
        tokens!.AccessToken.Should().NotBeNullOrWhiteSpace();
        tokens.RefreshToken.Should().NotBeNullOrWhiteSpace();
        tokens.TokenType.Should().Be("Bearer");
        tokens.AccessTokenExpiresInSeconds.Should().BePositive();
        tokens.AccessTokenExpiresAtUtc.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));

        var logoutResponse = await _client.PostAsJsonAsync("/api/auth/logout", new RefreshTokenRequest(tokens.RefreshToken));
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(tokens.RefreshToken));
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithActiveRefreshToken_ReturnsNewTokens()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("refresh@example.com", AuthEndpointsWebApplicationFactory.TestPassword));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var tokens = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        tokens.Should().NotBeNull();
        tokens!.AccessToken.Should().NotBeNullOrWhiteSpace();
        tokens.RefreshToken.Should().NotBeNullOrWhiteSpace();
        tokens.TokenType.Should().Be("Bearer");
        tokens.AccessTokenExpiresInSeconds.Should().BePositive();
        tokens.AccessTokenExpiresAtUtc.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));

        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(tokens.RefreshToken));

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshed = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>();
        refreshed.Should().NotBeNull();
        refreshed!.AccessToken.Should().NotBeNullOrWhiteSpace();
        refreshed.RefreshToken.Should().NotBeNullOrWhiteSpace();
        refreshed.TokenType.Should().Be("Bearer");
        refreshed.RefreshToken.Should().NotBe(tokens.RefreshToken);
        refreshed.AccessTokenExpiresInSeconds.Should().BePositive();
        refreshed.AccessTokenExpiresAtUtc.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task Refresh_WithUnknownRefreshToken_ReturnsUnauthorized()
    {
        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest("invalid-refresh-token"));

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record LoginRequest(string Email, string Password);
    private sealed record RefreshTokenRequest(string RefreshToken);
    private sealed record AuthResponse(
        string AccessToken,
        string RefreshToken,
        string TokenType,
        int AccessTokenExpiresInSeconds,
        DateTimeOffset AccessTokenExpiresAtUtc);
}
