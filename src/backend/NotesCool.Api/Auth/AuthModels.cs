using Microsoft.EntityFrameworkCore;

namespace NotesCool.Api.Auth;

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record AuthTokenResponse(string AccessToken, string RefreshToken, string TokenType, int ExpiresIn);

public sealed record AuthErrorResponse(string Error, string Message);

public sealed class UserAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string Status { get; set; } = UserAccountStatuses.Active;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public static class UserAccountStatuses
{
    public const string Active = "active";
}

public sealed record RegisterRequest(string Email, string Password);

public sealed record RegisterResponse(Guid Id, string Email, string Status);

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<UserAccount> Users => Set<UserAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.ToTable("user_accounts");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).HasMaxLength(320).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Status).HasMaxLength(32).IsRequired();
            entity.Property(u => u.CreatedAt).IsRequired();
        });
    }
}
