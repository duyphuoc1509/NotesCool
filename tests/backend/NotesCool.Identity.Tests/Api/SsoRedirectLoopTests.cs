using System.Net;
using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NotesCool.Api.Configuration;
using NotesCool.Api.Identity;
using Xunit;

namespace NotesCool.Identity.Tests.Api;

public class SsoRedirectLoopTests
{
    [Fact]
    public void BuildFrontendCallbackRedirectUrl_ShouldHandleTrailingSlashInRedirectUrls()
    {
        var options = new SsoProviderOptions
        {
            // API callback URL (misconfigured in RedirectUrls with a trailing slash)
            RedirectUrls = ["https://note.tripschill.com/api/auth/sso/google/callback/"]
        };

        var method = typeof(GoogleSsoExtensions)
            .GetMethod("BuildFrontendCallbackRedirectUrl", BindingFlags.NonPublic | BindingFlags.Static);

        method.Should().NotBeNull();

        var redirectUrl = (string)method!.Invoke(null, [options, "session-code-123", NullLogger.Instance])!;

        // It should NOT be the same as the input URL (which would cause a loop)
        redirectUrl.Should().NotStartWith("https://note.tripschill.com/api/auth/sso/google/callback/");
        
        // It should have been corrected to a frontend URL
        redirectUrl.Should().StartWith("https://note.tripschill.com/auth/sso/google/callback?");
    }
}
