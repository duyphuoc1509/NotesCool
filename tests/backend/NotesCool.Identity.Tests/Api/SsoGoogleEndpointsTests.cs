using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace NotesCool.Identity.Tests.Api;

public class SsoGoogleEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SsoGoogleEndpointsTests(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("SSO_GOOGLE_ENABLED", "true");
        Environment.SetEnvironmentVariable("SSO_GOOGLE_CLIENT_ID", "test-client-id");
        Environment.SetEnvironmentVariable("SSO_GOOGLE_CLIENT_SECRET", "test-client-secret");
        Environment.SetEnvironmentVariable("SSO_GOOGLE_REDIRECT_URLS", "https://note.tripschill.com/auth/callback/google");
        _factory = factory;
    }

    [Fact]
    public async Task Get_Login_WithXForwardedProto_ShouldReturnHttpsRedirect()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/sso/google/login");
        request.Headers.Add("X-Forwarded-Proto", "https");
        request.Headers.Add("X-Forwarded-Host", "note.tripschill.com");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        var location = response.Headers.Location!.ToString();
        
        // This is the core issue: if it returns http://... instead of https://..., 
        // Google will reject it if the registered URI is https://...
        location.Should().Contain("redirect_uri=https%3A%2F%2Fnote.tripschill.com%2Fapi%2Fauth%2Fsso%2Fgoogle%2Fcallback");
    }
}
