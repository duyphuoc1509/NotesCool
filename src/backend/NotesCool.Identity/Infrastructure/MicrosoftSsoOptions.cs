namespace NotesCool.Identity.Infrastructure;

public sealed class MicrosoftSsoOptions
{
    public const string SectionName = "Authentication:Microsoft";

    public string TenantId { get; set; } = "common";

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    public string CallbackPath { get; set; } = "/api/auth/sso/microsoft/callback";

    public string[] Scopes { get; set; } = ["openid", "profile", "email", "User.Read"];

    public string Authority => $"https://login.microsoftonline.com/{TenantId}";

    public string AuthorizationEndpoint => $"{Authority}/oauth2/v2.0/authorize";

    public string TokenEndpoint => $"{Authority}/oauth2/v2.0/token";
}
