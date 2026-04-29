using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NotesCool.Api.Identity;
using NotesCool.Identity.Infrastructure;
using NotesCool.Shared.Auth;
using Xunit;

namespace NotesCool.Notes.Tests.Api;

public class SsoAccountLinkingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SsoAccountLinkingTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                var dbName = $"IdentityDb-{Guid.NewGuid()}";
                var optionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<IdentityDbContext>));
                if (optionsDescriptor is not null) services.Remove(optionsDescriptor);
                var contextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IdentityDbContext));
                if (contextDescriptor is not null) services.Remove(contextDescriptor);
                services.AddDbContext<IdentityDbContext>(options => options.UseInMemoryDatabase(dbName));

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, DynamicSsoTestAuthHandler>("Test", _ => { });
                services.AddScoped<ICurrentUser, CurrentUser>();
            });
        });
    }

    [Fact]
    public async Task LinkProvider_ReturnsLinkedProviderInCurrentAccount()
    {
        var client = CreateClient("user-a");
        var state = CreateState("github");

        var linkResponse = await client.PostAsJsonAsync("/api/auth/sso/me/providers", new LinkSsoProviderRequest(
            "github",
            "valid-code",
            state,
            "github-user-1",
            "user-a@example.com",
            "User A"));

        linkResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await linkResponse.Content.ReadFromJsonAsync<SsoUserResponse>();
        payload.Should().NotBeNull();
        payload!.UserId.Should().Be("user-a");
        payload.LinkedProviders.Should().ContainSingle(x => x.Provider == "github" && x.ProviderUserId == "github-user-1");

        var providersResponse = await client.GetAsync("/api/auth/sso/me/providers");
        providersResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var providers = await providersResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<LinkedSsoProviderResponse>>();
        providers.Should().ContainSingle(x => x.Provider == "github" && x.ProviderUserId == "github-user-1");
    }

    [Fact]
    public async Task LinkProvider_WhenProviderIdentityAlreadyLinkedToAnotherAccount_ReturnsConflict()
    {
        var clientA = CreateClient("user-a");
        var firstState = CreateState("google");
        var firstLinkResponse = await clientA.PostAsJsonAsync("/api/auth/sso/me/providers", new LinkSsoProviderRequest(
            "google",
            "valid-code",
            firstState,
            "google-user-1",
            "user-a@example.com",
            "User A"));
        firstLinkResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var clientB = CreateClient("user-b");
        var duplicateState = CreateState("google");
        var duplicateLinkResponse = await clientB.PostAsJsonAsync("/api/auth/sso/me/providers", new LinkSsoProviderRequest(
            "google",
            "valid-code",
            duplicateState,
            "google-user-1",
            "user-b@example.com",
            "User B"));

        duplicateLinkResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var error = await duplicateLinkResponse.Content.ReadFromJsonAsync<SsoErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Be("provider_identity_in_use");
    }

    [Fact]
    public async Task Callback_WhenValidIdentity_CreatesUserAndReturnsBearerToken()
    {
        var client = _factory.CreateClient();
        var state = CreateState("google");

        var response = await client.PostAsJsonAsync("/api/auth/sso/callback", new SsoCallbackRequest(
            "google",
            "valid-code",
            state,
            "callback@example.com",
            "google-user-123",
            "Callback User"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<SsoTokenResponse>();
        payload.Should().NotBeNull();
        payload!.TokenType.Should().Be("Bearer");
        payload.AccessToken.Should().NotBeNullOrWhiteSpace();
        payload.ExpiresIn.Should().Be(900);
        payload.RefreshToken.Should().NotBeNullOrWhiteSpace();
        payload.User.UserId.Should().NotBeNullOrEmpty();
        payload.User.Email.Should().Be("callback@example.com");
        payload.User.LinkedProviders.Should().ContainSingle(x => x.Provider == "google" && x.ProviderUserId == "google-user-123");
    }

    [Fact]
    public async Task Callback_WhenProviderIdentityAlreadyExists_ReturnsExistingUser()
    {
        var client = _factory.CreateClient();
        var firstState = CreateState("google");

        var firstResponse = await client.PostAsJsonAsync("/api/auth/sso/callback", new SsoCallbackRequest(
            "google",
            "valid-code",
            firstState,
            "callback@example.com",
            "google-user-123",
            "Callback User"));
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstPayload = await firstResponse.Content.ReadFromJsonAsync<SsoTokenResponse>();
        var userId = firstPayload!.User.UserId;
        var secondState = CreateState("google");

        var secondResponse = await client.PostAsJsonAsync("/api/auth/sso/callback", new SsoCallbackRequest(
            "google",
            "another-code",
            secondState,
            "different@example.com",
            "google-user-123",
            "Another Name"));

        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await secondResponse.Content.ReadFromJsonAsync<SsoTokenResponse>();
        payload.Should().NotBeNull();
        payload!.User.UserId.Should().Be(userId);
        payload.User.LinkedProviders.Should().ContainSingle(x => x.Provider == "google" && x.ProviderUserId == "google-user-123");
    }

    [Theory]
    [InlineData("", "valid-code", "google-user-123", false)]
    [InlineData("google", "", "google-user-123", false)]
    [InlineData("google", "valid-code", "google-user-123", true)]
    [InlineData("google", "valid-code", "", false)]
    public async Task Callback_WhenRequestIsInvalid_ReturnsBadRequest(string provider, string code, string providerUserId, bool useInvalidState)
    {
        var client = _factory.CreateClient();
        var state = CreateState("google");
        var requestedState = useInvalidState ? "invalid-state" : state;

        var response = await client.PostAsJsonAsync("/api/auth/sso/callback", new SsoCallbackRequest(
            provider,
            code,
            requestedState,
            "callback@example.com",
            providerUserId,
            "Callback User"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var payload = await response.Content.ReadFromJsonAsync<SsoErrorResponse>();
        payload.Should().NotBeNull();
        payload!.Error.Should().Be("invalid_sso_callback");
    }

    [Fact]
    public async Task UnlinkProvider_WhenAnotherMethodExists_RemovesProvider()
    {
        var client = CreateClient("user-a");
        var githubState = CreateState("github");

        var githubLinkResponse = await client.PostAsJsonAsync("/api/auth/sso/me/providers", new LinkSsoProviderRequest(
            "github",
            "valid-code",
            githubState,
            "github-user-1",
            "user-a@example.com",
            "User A"));
        githubLinkResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var googleState = CreateState("google");

        var googleLinkResponse = await client.PostAsJsonAsync("/api/auth/sso/me/providers", new LinkSsoProviderRequest(
            "google",
            "valid-code",
            googleState,
            "google-user-1",
            "user-a@example.com",
            "User A"));
        googleLinkResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var unlinkResponse = await client.DeleteAsync("/api/auth/sso/me/providers/github");

        unlinkResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var providersResponse = await client.GetAsync("/api/auth/sso/me/providers");
        providersResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var providers = await providersResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<LinkedSsoProviderResponse>>();
        providers.Should().ContainSingle(x => x.Provider == "google" && x.ProviderUserId == "google-user-1");
        providers.Should().NotContain(x => x.Provider == "github");
    }

    [Fact]
    public async Task UnlinkProvider_WhenRemovingLastLoginMethod_ReturnsBadRequest()
    {
        var client = CreateClient("user-a");
        var state = CreateState("google");

        var linkResponse = await client.PostAsJsonAsync("/api/auth/sso/me/providers", new LinkSsoProviderRequest(
            "google",
            "valid-code",
            state,
            "google-user-1",
            "user-a@example.com",
            "User A"));
        linkResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var unlinkResponse = await client.DeleteAsync("/api/auth/sso/me/providers/google");

        unlinkResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await unlinkResponse.Content.ReadFromJsonAsync<SsoErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Be("sso_unlink_denied");
        error.Message.Should().Be("At least one login method must remain linked.");
    }

    [Fact]
    public async Task UnlinkProvider_WhenProviderIsNotLinked_ReturnsBadRequest()
    {
        var client = CreateClient("user-a");
        var state = CreateState("google");

        var linkResponse = await client.PostAsJsonAsync("/api/auth/sso/me/providers", new LinkSsoProviderRequest(
            "google",
            "valid-code",
            state,
            "google-user-1",
            "user-a@example.com",
            "User A"));
        linkResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var unlinkResponse = await client.DeleteAsync("/api/auth/sso/me/providers/github");

        unlinkResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await unlinkResponse.Content.ReadFromJsonAsync<SsoErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Be("sso_unlink_denied");
        error.Message.Should().Be("Provider is not linked to this account.");
    }

    private HttpClient CreateClient(string userId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", userId);
        return client;
    }

    private string CreateState(string provider)
    {
        var store = _factory.Services.GetRequiredService<SsoStore>();
        return store.CreateState(provider, "http://localhost");
    }
}
