using Microsoft.AspNetCore.Http.HttpResults;

namespace NotesCool.Api.Auth;

public static class AuthEndpointExtensions
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder builder)
    {
        var authGroup = builder.MapGroup("api/auth").WithTags("Auth");

        authGroup.MapPost("login", async Task<Results<Ok<LoginResponse>, UnauthorizedHttpResult>> (
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

        return builder;
    }
}
