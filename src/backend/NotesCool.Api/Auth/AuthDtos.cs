namespace NotesCool.Api.Auth;

public sealed record LoginRequest(string Email, string Password, int? RefreshTokenLifetimeSeconds = null);

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    AuthUserResponse User);

public sealed record AuthUserResponse(string Id, string Email, string DisplayName);

public sealed record RegisteredUser(string Id, string Email, string DisplayName, string Password);
