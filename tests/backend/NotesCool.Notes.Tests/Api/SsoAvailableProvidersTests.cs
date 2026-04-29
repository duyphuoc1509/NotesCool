using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
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
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"AUTH_GOOGLE_ENABLED", "true"},
                    {"AUTH_MICROSOFT_ENABLED", "false"}
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