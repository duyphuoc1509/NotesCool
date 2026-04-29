using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using NotesCool.Api.Configuration;
using Xunit;
using FluentAssertions;
using System.Linq;

namespace NotesCool.Notes.Tests.Api;

public class SsoConfigValidationTests
{
    private readonly MockHostEnvironment _env = new() { EnvironmentName = "Production" };
    private readonly SsoOptionsValidator _validator;

    public SsoConfigValidationTests()
    {
        _validator = new SsoOptionsValidator(_env);
    }

    [Fact]
    public void Validator_WhenProviderEnabledWithoutClientId_ReturnsFailure()
    {
        var options = new SsoOptions
        {
            Providers = new List<SsoProviderOptions>
            {
                new() { Name = "Google", Enabled = true, ClientId = "", ClientSecret = "secret", Authority = "https://accounts.google.com", CallbackPath = "/signin-google", RedirectUrls = ["https://example.com/callback"] }
            }
        };

        var result = _validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(f => f.Contains("ClientId is not configured"));
    }

    [Fact]
    public void Validator_WhenProviderEnabledWithPlaceholderSecret_ReturnsFailure()
    {
        var options = new SsoOptions
        {
            Providers = new List<SsoProviderOptions>
            {
                new() { Name = "Google", Enabled = true, ClientId = "id", ClientSecret = "__SET_IN_ENV__", Authority = "https://accounts.google.com", CallbackPath = "/signin-google", RedirectUrls = ["https://example.com/callback"] }
            }
        };

        var result = _validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(f => f.Contains("ClientSecret is empty or still uses a placeholder"));
    }
}
