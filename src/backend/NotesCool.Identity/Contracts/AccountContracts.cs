namespace NotesCool.Identity.Contracts;

public sealed record RegisterRequest(string Email, string Password, string DisplayName);

public sealed record LoginRequest(string Email, string Password);

public sealed record UserProfileResponse(string Id, string Email, string DisplayName, string Status);

public sealed record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, UserProfileResponse User);
