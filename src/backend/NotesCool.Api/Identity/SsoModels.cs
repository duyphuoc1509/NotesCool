namespace NotesCool.Api.Identity;

public sealed record SsoCallbackRequest(string Provider, string Code, string State, string? Email, string? ProviderUserId, string? DisplayName);

public sealed record LinkSsoProviderRequest(string Provider, string Code, string State, string ProviderUserId, string? Email, string? DisplayName);

public sealed record SsoTokenResponse(string AccessToken, string TokenType, int ExpiresIn, SsoUserResponse User);

public sealed record SsoUserResponse(string UserId, string? Email, string? DisplayName, IReadOnlyCollection<LinkedSsoProviderResponse> LinkedProviders);

public sealed record LinkedSsoProviderResponse(string Provider, string ProviderUserId, string? Email, DateTimeOffset LinkedAt);

public sealed record SsoErrorResponse(string Error, string Message);
