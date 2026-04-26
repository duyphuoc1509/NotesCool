using Microsoft.AspNetCore.Http.HttpResults;

namespace NotesCool.Api.Auth;

public static class AuthEndpointExtensions
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/auth").WithTags("Auth");

        group.MapPost("login", async Task<Results<Ok<LoginResponse>, UnauthorizedHttpResult>> (
            LoginRequest request,
            IUserCredentialStore credentialStore,
            IAccessTokenService tokenService,
            CancellationToken cancellationToken) =>
        {
            var user = await credentialStore.ValidateAsync(request.Email, request.Password, cancellationToken);

            return user is null
                ? TypedResults.Unauthorized()
                : TypedResults.Ok(tokenService.CreateLoginResponse(user));
        })
        .AllowAnonymous()
        .WithSummary("Login with email and password")
        .WithDescription("Valid credentials return a JWT access token and user info. Invalid credentials return a generic 401 response.")
        .Produces<LoginResponse>()
        .Produces(StatusCodes.Status401Unauthorized);

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
