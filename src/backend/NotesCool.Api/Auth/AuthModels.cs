namespace NotesCool.Api.Auth;

public sealed record LoginRequest(string Email, string Password, int? RefreshTokenLifetimeSeconds = null);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record AuthTokenResponse(string AccessToken, string RefreshToken, string TokenType, int ExpiresIn);

public sealed record AuthErrorResponse(string Error, string Message);
