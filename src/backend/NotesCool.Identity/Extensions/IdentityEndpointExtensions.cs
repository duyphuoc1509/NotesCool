using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NotesCool.Identity.Application;
using NotesCool.Identity.Contracts;

namespace NotesCool.Identity.Extensions;

public static class IdentityEndpointExtensions
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder builder)
    {
        var accountGroup = builder.MapGroup("api/account").WithTags("Account");

        accountGroup.MapPost("register", async (RegisterRequest request, AccountService service) =>
        {
            try
            {
                var result = await service.RegisterAsync(request);
                return Results.Created($"/api/account/profile", result);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
            {
                return Results.Conflict(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        accountGroup.MapPost("login", async (LoginRequest request, AccountService service) =>
        {
            try
            {
                var result = await service.LoginAsync(request);
                return Results.Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (AccountInactiveException)
            {
                return Results.Forbid();
            }
        });

        accountGroup.MapPost("logout", () => Results.NoContent()).RequireAuthorization();

        var ssoGroup = builder.MapGroup("api/auth/sso").WithTags("SSO");

        ssoGroup.MapGet("microsoft/login", (HttpContext context, SsoService service) =>
        {
            var redirectUri = BuildAbsoluteUri(context, "/api/auth/sso/microsoft/callback");
            var loginUrl = service.GetMicrosoftLoginUrl(redirectUri);
            return Results.Redirect(loginUrl);
        });

        ssoGroup.MapGet("microsoft/callback", async (string? code, HttpContext context, SsoService service) =>
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Results.BadRequest(new { error = "Missing Microsoft authorization code." });
            }

            try
            {
                var redirectUri = BuildAbsoluteUri(context, "/api/auth/sso/microsoft/callback");
                var result = await service.HandleMicrosoftCallbackAsync(code, redirectUri);
                return Results.Ok(result);
            }
            catch (AccountInactiveException)
            {
                return Results.Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        return builder;
    }

    private static string BuildAbsoluteUri(HttpContext context, string path)
    {
        return $"{context.Request.Scheme}://{context.Request.Host}{path}";
    }
}
