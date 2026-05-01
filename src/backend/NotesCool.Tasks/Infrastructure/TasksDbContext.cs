using Microsoft.EntityFrameworkCore;
using NotesCool.Tasks.Domain;
using TaskStatus = NotesCool.Tasks.Domain.TaskStatus;

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

            entity.Property(e => e.Status)
                .HasConversion(
                    status => (int)status,
                    dbValue => Enum.IsDefined(typeof(TaskStatus), dbValue)
                        ? (TaskStatus)dbValue
                        : TaskStatus.Todo);
                
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
