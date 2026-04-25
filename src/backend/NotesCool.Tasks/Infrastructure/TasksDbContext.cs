using Microsoft.EntityFrameworkCore;
using NotesCool.Tasks.Domain;

namespace NotesCool.Tasks.Infrastructure;

public sealed class TasksDbContext(DbContextOptions<TasksDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<TaskItem>(b =>
        {
            b.ToTable("tasks"); b.HasKey(x => x.Id);
            b.Property(x => x.OwnerId).HasMaxLength(128).IsRequired();
            b.Property(x => x.Title).HasMaxLength(200).IsRequired();
            b.HasIndex(x => new { x.OwnerId, x.Status, x.ArchivedAt });
        });
    }
}
