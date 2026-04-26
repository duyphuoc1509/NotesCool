using Microsoft.AspNetCore.Http.HttpResults;

namespace NotesCool.Api.Auth;

public static class AuthEndpointExtensions
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/auth").WithTags("Auth");

        group.MapPost("register", RegisterAsync)
            .AllowAnonymous()
            .WithSummary("Register a new user")
            .WithDescription("Creates an active account using email/password. Enforces password policy and rejects duplicate emails.")
            .Produces<RegisterResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict);

        return builder;
    }

    private static async Task<IResult> RegisterAsync(RegisterRequest request, RegistrationService service, CancellationToken cancellationToken)
    {
        try
        {
            var response = await service.RegisterAsync(request, cancellationToken);
            return Results.Created($"/api/users/{response.Id}", response);
        }
        catch (RegistrationValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (DuplicateEmailException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
    }
}
