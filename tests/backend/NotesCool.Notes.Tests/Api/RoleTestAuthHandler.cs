using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NotesCool.Notes.Tests.Api;

public class RoleTestAuthHandlerOptions : AuthenticationSchemeOptions
{
    public string Role { get; set; } = "User";
}

public class RoleTestAuthHandler : AuthenticationHandler<RoleTestAuthHandlerOptions>
{
    public const string AuthenticationScheme = "TestRoleScheme";

    public RoleTestAuthHandler(
        IOptionsMonitor<RoleTestAuthHandlerOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Name, "test-user"),
            new Claim(ClaimTypes.Role, Options.Role)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
