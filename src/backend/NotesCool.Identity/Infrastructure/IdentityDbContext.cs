using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace NotesCool.Identity.Infrastructure;

public sealed class IdentityDbContext : IdentityDbContext<ApplicationUser>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserExternalLogin> UserExternalLogins => Set<UserExternalLogin>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.DisplayName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
        });

        builder.Entity<UserExternalLogin>(entity =>
        {
            entity.ToTable("UserExternalLogins");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId)
                .IsRequired();

            entity.Property(x => x.ProviderKey)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(x => x.ProviderSubject)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(x => x.ProviderDisplayName)
                .HasMaxLength(256);

            entity.Property(x => x.Email)
                .HasMaxLength(256);

            entity.HasIndex(x => new { x.ProviderKey, x.ProviderSubject })
                .IsUnique();

            entity.HasIndex(x => x.UserId);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
