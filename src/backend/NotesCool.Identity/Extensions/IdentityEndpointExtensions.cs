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

        return builder;
    }
}
