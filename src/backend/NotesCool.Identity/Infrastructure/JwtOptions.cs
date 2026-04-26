namespace NotesCool.Identity.Infrastructure;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "NotesCool";
    public string Audience { get; set; } = "NotesCool.Client";
    public string SigningKey { get; set; } = "super-secret-signing-key-with-32-chars";
    public int ExpiresInMinutes { get; set; } = 60;
}
