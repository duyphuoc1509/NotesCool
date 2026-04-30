using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NotesCool.Api.Configuration;
using Xunit;

namespace NotesCool.Identity.Tests.Configuration;

public sealed class SsoEnvironmentConfigServiceTests : IDisposable
{
    private static readonly string[] Variables =
    [
        "SSO_GOOGLE_ENABLED",
        "SSO_GOOGLE_CLIENT_ID",
        "SSO_GOOGLE_CLIENT_SECRET",
        "SSO_GOOGLE_AUTHORITY",
        "SSO_GOOGLE_CALLBACK_PATH",
        "SSO_GOOGLE_REDIRECT_URLS",
        "GOOGLE_SSO_ENABLED",
        "GOOGLE_SSO_CLIENT_ID",
        "GOOGLE_SSO_CLIENT_SECRET",
        "GOOGLE_SSO_AUTHORITY",
        "GOOGLE_SSO_CALLBACK_PATH",
        "GOOGLE_SSO_REDIRECT_URLS",
        "SSO_MICROSOFT_ENABLED",
        "SSO_MICROSOFT_CLIENT_ID",
        "SSO_MICROSOFT_CLIENT_SECRET",
        "SSO_MICROSOFT_AUTHORITY",
        "SSO_MICROSOFT_CALLBACK_PATH",
        "SSO_MICROSOFT_REDIRECT_URLS"
    ];

    private readonly Dictionary<string, string?> _originalValues = Variables.ToDictionary(
        variable => variable,
        Environment.GetEnvironmentVariable);

    public SsoEnvironmentConfigServiceTests()
    {
        ClearVariables();
    }

    [Fact]
    public void GetOptions_ShouldNotRequireClientCredentials_WhenProvidersAreDisabled()
    {
        var service = new SsoEnvironmentConfigService(NullLogger<SsoEnvironmentConfigService>.Instance);

        var options = service.GetOptions();

        options.Providers.Should().HaveCount(2);
        options.Providers.Should().OnlyContain(provider => !provider.Enabled);
        options.Providers.Should().OnlyContain(provider => string.IsNullOrWhiteSpace(provider.ClientId));
        options.Providers.Should().OnlyContain(provider => string.IsNullOrWhiteSpace(provider.ClientSecret));
    }

    [Fact]
    public void GetOptions_ShouldReadGoogleProviderConfig_FromSsoGoogleEnvironmentVariablesOnly()
    {
        Environment.SetEnvironmentVariable("SSO_GOOGLE_ENABLED", "true");
        Environment.SetEnvironmentVariable("SSO_GOOGLE_CLIENT_ID", "google-client-id");
        Environment.SetEnvironmentVariable("SSO_GOOGLE_CLIENT_SECRET", "google-client-secret");
        Environment.SetEnvironmentVariable("SSO_GOOGLE_AUTHORITY", "https://accounts.google.test");
        Environment.SetEnvironmentVariable("SSO_GOOGLE_CALLBACK_PATH", "/custom-google-callback");
        Environment.SetEnvironmentVariable("SSO_GOOGLE_REDIRECT_URLS", "https://app.test/google, https://admin.test/google");

        var service = new SsoEnvironmentConfigService(NullLogger<SsoEnvironmentConfigService>.Instance);

        var options = service.GetOptions();
        var googleProvider = options.Providers.Single(provider => provider.Name == "Google");

        googleProvider.Enabled.Should().BeTrue();
        googleProvider.ClientId.Should().Be("google-client-id");
        googleProvider.ClientSecret.Should().Be("google-client-secret");
        googleProvider.Authority.Should().Be("https://accounts.google.test");
        googleProvider.CallbackPath.Should().Be("/custom-google-callback");
        googleProvider.RedirectUrls.Should().Equal("https://app.test/google", "https://admin.test/google");
    }

    [Fact]
    public void GetOptions_ShouldIgnoreGoogleSsoLegacyEnvironmentVariables()
    {
        Environment.SetEnvironmentVariable("GOOGLE_SSO_ENABLED", "true");
        Environment.SetEnvironmentVariable("GOOGLE_SSO_CLIENT_ID", "google-client-id");
        Environment.SetEnvironmentVariable("GOOGLE_SSO_CLIENT_SECRET", "google-client-secret");

        var service = new SsoEnvironmentConfigService(NullLogger<SsoEnvironmentConfigService>.Instance);

        var options = service.GetOptions();
        var googleProvider = options.Providers.Single(provider => provider.Name == "Google");

        googleProvider.Enabled.Should().BeFalse();
        googleProvider.ClientId.Should().BeEmpty();
        googleProvider.ClientSecret.Should().BeEmpty();
    }

    [Fact]
    public void GetOptions_ShouldIgnoreSsoProvidersConfigurationArray()
    {
        var service = new SsoEnvironmentConfigService(NullLogger<SsoEnvironmentConfigService>.Instance);

        var options = service.GetOptions();
        var googleProvider = options.Providers.Single(provider => provider.Name == "Google");

        googleProvider.Enabled.Should().BeFalse();
        googleProvider.ClientId.Should().BeEmpty();
        googleProvider.ClientSecret.Should().BeEmpty();
    }

    [Fact]
    public void Validator_ShouldFail_WhenEnabledProviderHasMissingRequiredVariables()
    {
        var provider = new SsoProviderOptions
        {
            Name = "Google",
            Enabled = true,
            Authority = "https://accounts.google.com",
            CallbackPath = "/signin-google",
            RedirectUrls = ["https://app.test/auth/callback/google"]
        };
        var options = new SsoOptions { Providers = [provider] };
        var validator = new SsoOptionsValidator(new TestHostEnvironment());

        var result = validator.Validate(Options.DefaultName, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(failure => failure.Contains("ClientId"));
        result.Failures.Should().Contain(failure => failure.Contains("ClientSecret"));
    }

    [Fact]
    public void Validator_ShouldSucceed_WhenDisabledProviderHasMissingCredentials()
    {
        var provider = new SsoProviderOptions
        {
            Name = "Google",
            Enabled = false
        };
        var options = new SsoOptions { Providers = [provider] };
        var validator = new SsoOptionsValidator(new TestHostEnvironment());

        var result = validator.Validate(Options.DefaultName, options);

        result.Succeeded.Should().BeTrue();
    }

    public void Dispose()
    {
        foreach (var (variable, originalValue) in _originalValues)
        {
            Environment.SetEnvironmentVariable(variable, originalValue);
        }
    }

    private static void ClearVariables()
    {
        foreach (var variable in Variables)
        {
            Environment.SetEnvironmentVariable(variable, null);
        }
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "NotesCool.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
