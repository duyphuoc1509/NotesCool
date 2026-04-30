using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NotesCool.Api.Identity;
using Xunit;

namespace NotesCool.Identity.Tests.Api;

public class SsoCallbackAuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SsoCallbackAuthorizationTests(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("SSO_GOOGLE_ENABLED", "true");
        Environment.SetEnvironmentVariable("SSO_GOOGLE_CLIENT_ID", "test-client-id");
        Environment.SetEnvironmentVariable("SSO_GOOGLE_CLIENT_SECRET", "test-client-secret");
        Environment.SetEnvironmentVariable("SSO_GOOGLE_REDIRECT_URLS", "https://note.tripschill.com/auth/callback/google");
        _factory = factory;
    }

    [Fact]
    public async Task Callback_IssuesToken_ThatCanAccessProtectedSsoEndpoint()
    {
        var app = CreateApp($"IdentityDb-{Guid.NewGuid()}", $"NotesDb-{Guid.NewGuid()}", $"TasksDb-{Guid.NewGuid()}");
        using var client = app.CreateClient();

        await using var scope = app.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<SsoStore>();
        var state = store.CreateState("Google", "https://note.tripschill.com/auth/callback/google");

        var callbackResponse = await client.PostAsJsonAsync("/api/auth/sso/callback", new SsoCallbackRequest(
            Provider: "Google",
            Code: "valid-code",
            State: state,
            Email: "sso@example.com",
            ProviderUserId: "google-sub-123",
            DisplayName: "SSO User"));

        callbackResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokenPayload = await callbackResponse.Content.ReadFromJsonAsync<SsoTokenResponse>();
        tokenPayload.Should().NotBeNull();
        tokenPayload!.AccessToken.Should().NotBeNullOrWhiteSpace();
        tokenPayload.RefreshToken.Should().NotBeNullOrWhiteSpace();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenPayload.AccessToken);
        var providersResponse = await client.GetAsync("/api/auth/sso/me/providers");

        providersResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var providers = await providersResponse.Content.ReadFromJsonAsync<List<LinkedSsoProviderResponse>>();
        providers.Should().NotBeNull();
        providers!.Should().ContainSingle(p => p.Provider == "google" && p.ProviderUserId == "google-sub-123");
    }

    private WebApplicationFactory<Program> CreateApp(string identityDbName, string notesDbName, string tasksDbName)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                ReplaceDbContext<NotesCool.Identity.Infrastructure.IdentityDbContext>(services, identityDbName);
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
