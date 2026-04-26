using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NotesCool.Api.Identity;
using NotesCool.Shared.Auth;
using Xunit;

namespace NotesCool.Notes.Tests.Api;

public class SsoEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SsoEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<SsoStore>();
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
                services.AddScoped<ICurrentUser, TestCurrentUser>();
            });
        });
    }

    [Fact]
    public async Task Callback_WithValidPayload_ReturnsAccessTokenAndLinkedProvider()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/sso/callback", new SsoCallbackRequest(
            "google",
            "valid-code",
            "sso_state_123",
            "alice@example.com",
            "google-user-1",
            "Alice"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<SsoTokenResponse>();
        payload.Should().NotBeNull();
        payload!.AccessToken.Should().NotBeNullOrWhiteSpace();
        payload.User.Email.Should().Be("alice@example.com");
        payload.User.LinkedProviders.Should().ContainSingle(x => x.Provider == "google" && x.ProviderUserId == "google-user-1");
    }

    [Fact]
    public async Task Callback_WithInvalidState_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/sso/callback", new SsoCallbackRequest(
            "google",
            "valid-code",
            "invalid-state",
            "alice@example.com",
            "google-user-1",
            "Alice"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LinkAndUnlinkProvider_RejectsRemovingLastLoginMethod()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        var linkResponse = await client.PostAsJsonAsync("/api/auth/sso/providers", new LinkSsoProviderRequest(
            "github",
            "valid-code",
            "sso_state_123",
            "github-user-1",
            "test@example.com",
            "Tester"));

        linkResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var unlinkResponse = await client.DeleteAsync("/api/auth/sso/providers/github");
        unlinkResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await unlinkResponse.Content.ReadFromJsonAsync<SsoErrorResponse>();
        error.Should().NotBeNull();
        error!.Message.Should().Contain("At least one login method must remain linked");
    }

    [Fact]
    public void Store_RejectsLinkingSameProviderIdentityToDifferentUsers()
    {
        var store = new SsoStore();
        store.LinkProvider("user-a", "google", "google-user-1", "a@example.com", "A");

        var act = () => store.LinkProvider("user-b", "google", "google-user-1", "b@example.com", "B");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already linked to another account*");
    }
}
