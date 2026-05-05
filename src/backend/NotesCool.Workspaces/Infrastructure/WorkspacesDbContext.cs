using Microsoft.EntityFrameworkCore;
using NotesCool.Workspaces.Domain;

namespace NotesCool.Workspaces.Infrastructure;

public sealed class WorkspacesDbContext(DbContextOptions<WorkspacesDbContext> options) : DbContext(options)
{
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Workspace>(b =>
        {
            b.ToTable("workspaces");
            b.HasKey(x => x.Id);
            b.Property(x => x.OwnerId).HasMaxLength(128).IsRequired();
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.HasQueryFilter(x => x.ArchivedAt == null);
            b.HasIndex(x => new { x.OwnerId, x.ArchivedAt });
            
            b.HasMany(x => x.Members)
             .WithOne()
             .HasForeignKey(x => x.WorkspaceId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WorkspaceMember>(b =>
        {
            b.ToTable("workspace_members");
            b.HasKey(x => new { x.WorkspaceId, x.UserId });
            b.Property(x => x.WorkspaceId).IsRequired();
            b.Property(x => x.UserId).HasMaxLength(128).IsRequired();
            b.Property(x => x.Role).HasConversion<int>();
        });
    }
}
