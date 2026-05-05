using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotesCool.Api.Configuration;
using NotesCool.Api.Identity;
using Xunit;

namespace NotesCool.Notes.Tests.Api;

public class SsoAvailableProvidersTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SsoAvailableProvidersTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProviders_ReturnsConfiguredProviders()
    {
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureTestServices(services =>
            {
                services.PostConfigure<SsoOptions>(options =>
                {
                    options.Providers.Clear();
                    options.Providers.Add(new SsoProviderOptions
                    {
                        Name = "Google",
                        Enabled = true,
                        ClientId = "test-google-client",
                        ClientSecret = "test-google-secret",
                        Authority = "https://accounts.google.com",
                        CallbackPath = "/signin-google",
                        RedirectUri = "https://localhost:10001/auth/callback/google",
                        RedirectUrls = ["https://localhost/auth/callback/google"]
                    });
                });
            });
        });

        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auth/sso/providers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var providers = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<SsoAvailableProviderResponse>>();
        
        providers.Should().NotBeNull();
        providers.Should().ContainSingle();
        providers!.First().Key.Should().Be("google");
        providers!.First().Enabled.Should().BeTrue();
    }
}