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
using NotesCool.Api.Configuration;
using NotesCool.Api.Identity;
using NotesCool.Identity.Application;
using NotesCool.Identity.Infrastructure;
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
            builder.UseEnvironment("Testing");
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<SsoStore>();
                services.Configure<SsoOptions>(options =>
                {
                    options.Providers.Clear();
                    options.Providers.Add(new SsoProviderOptions { Name = "google", Enabled = true, ClientId = "test-client", ClientSecret = "test-secret", Authority = "https://accounts.example.com", CallbackPath = "/api/auth/sso/callback", RedirectUri = "https://localhost:10001/auth/callback/google", RedirectUrls = ["https://localhost/auth/callback/google"] });
                    options.Providers.Add(new SsoProviderOptions { Name = "github", Enabled = true, ClientId = "test-client", ClientSecret = "test-secret", Authority = "https://accounts.example.com", CallbackPath = "/api/auth/sso/callback", RedirectUri = "https://localhost:10001/auth/callback/github", RedirectUrls = ["https://localhost/auth/callback/github"] });
                });
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
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
                services.AddScoped<ICurrentUser, TestCurrentUser>();
            });
        });
    }

    [Fact]
    public async Task Callback_WithValidPayload_ReturnsAccessTokenAndLinkedProvider()
    {
        var client = _factory.CreateClient();
        var store = _factory.Services.GetRequiredService<SsoStore>();
        var state = store.CreateState("google", "http://localhost");

        var response = await client.PostAsJsonAsync("/api/auth/sso/callback", new SsoCallbackRequest(
            "google",
            "valid-code",
            state,
            "alice@example.com",
            "google-user-1",
            "Alice"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<SsoTokenResponse>();
        payload.Should().NotBeNull();
        payload!.AccessToken.Should().NotBeNullOrWhiteSpace();
        payload.RefreshToken.Should().NotBeNullOrWhiteSpace();
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
        var store = _factory.Services.GetRequiredService<SsoStore>();
        var state = store.CreateState("github", "http://localhost");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        var linkResponse = await client.PostAsJsonAsync("/api/auth/sso/me/providers", new LinkSsoProviderRequest(
            "github",
            "valid-code",
            state,
            "github-user-1",
            "test@example.com",
            "Tester"));

        linkResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var unlinkResponse = await client.DeleteAsync("/api/auth/sso/me/providers/github");
        unlinkResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await unlinkResponse.Content.ReadFromJsonAsync<SsoErrorResponse>();
        error.Should().NotBeNull();
        error!.Message.Should().Contain("At least one login method must remain linked");
    }

    [Fact]
    public void Store_RejectsLinkingSameProviderIdentityToDifferentUsers()
    {
        var store = new SsoStore();
        store.GetOrCreateUser("google", "123", "a@example.com", "A");
        
        var ex = Assert.Throws<InvalidOperationException>(() => 
            store.LinkProvider("user-b", "google", "123", "b@example.com", "B"));
            
        ex.Message.Should().Contain("already linked to another account");
    }
}
