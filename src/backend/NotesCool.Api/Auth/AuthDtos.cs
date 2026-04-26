namespace NotesCool.Api.Auth;

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    AuthUserResponse User);

public sealed record AuthUserResponse(string Id, string Email, string DisplayName);

public sealed record RegisteredUser(string Id, string Email, string DisplayName, string Password);
