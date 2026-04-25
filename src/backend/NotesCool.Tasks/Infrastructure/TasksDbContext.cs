using Microsoft.EntityFrameworkCore;
using NotesCool.Tasks.Domain;

namespace NotesCool.Tasks.Infrastructure;

public class TasksDbContext : DbContext
{
    public TasksDbContext(DbContextOptions<TasksDbContext> options) : base(options)
    {
    }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("Tasks");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);
                
            entity.Property(e => e.OwnerId)
                .IsRequired()
                .HasMaxLength(100);
                
            // Apply soft delete query filter globally
            entity.HasQueryFilter(e => e.ArchivedAt == null);
            
            // Indexes for performance on frequently filtered fields
            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DueDate);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
