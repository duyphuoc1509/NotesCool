using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace NotesCool.Identity.Tests.Api;

public class SsoMicrosoftCallbackTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SsoMicrosoftCallbackTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Callback_WithoutCode_ShouldReturnBadRequest()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/api/auth/sso/microsoft/callback");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
