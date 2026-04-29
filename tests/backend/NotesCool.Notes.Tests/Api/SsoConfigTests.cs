using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NotesCool.Api.Configuration;
using Xunit;
using FluentAssertions;

namespace NotesCool.Notes.Tests.Api;

public class SsoConfigTests
{
    [Fact]
    public void Options_WhenSsoSectionIsMissing_ShouldNotThrow()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IHostEnvironment>(new MockHostEnvironment { EnvironmentName = "Production" });
        services.AddOptions<SsoOptions>()
            .Bind(configuration.GetSection("Sso"))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<SsoOptions>, SsoOptionsValidator>();

        var provider = services.BuildServiceProvider();

        var act = () => provider.GetRequiredService<IOptions<SsoOptions>>().Value;

        act.Should().NotThrow();
        var options = act();
        options.Providers.Should().BeEmpty();
    }
}

internal class MockHostEnvironment : IHostEnvironment
{
    public string EnvironmentName { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string ContentRootPath { get; set; } = string.Empty;
    public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
}
