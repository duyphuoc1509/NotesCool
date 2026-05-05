using Microsoft.EntityFrameworkCore;
using NotesCool.Tasks.Domain.Entities;
using NotesCool.Tasks.Domain.Enums;
using TaskStatus = NotesCool.Tasks.Domain.Enums.TaskStatus;

namespace NotesCool.Tasks.Infrastructure;

public class TasksDbContext : DbContext
{
    public TasksDbContext(DbContextOptions<TasksDbContext> options) : base(options)
    {
    }

    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskAssignee> TaskAssignees => Set<TaskAssignee>();
    public DbSet<TaskActivityLog> TaskActivityLogs => Set<TaskActivityLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Workspace>(entity =>
        {
            entity.ToTable("Workspaces");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.OwnerId).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<WorkspaceMember>(entity =>
        {
            entity.ToTable("WorkspaceMembers");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.WorkspaceId, e.UserId }).IsUnique();
            entity.Property(e => e.Role).HasConversion<int>();
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Projects");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.OwnerId).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.WorkspaceId);
        });

        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.ToTable("ProjectMembers");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ProjectId, e.UserId }).IsUnique();
            entity.Property(e => e.Role).HasConversion<int>();
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("Tasks");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Title).IsRequired().HasMaxLength(250);
            entity.Property(e => e.OwnerId).IsRequired().HasMaxLength(100);

            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.Priority).HasConversion<int>();
                
            entity.HasQueryFilter(e => e.DeletedAt == null && e.ArchivedAt == null);
            
            entity.HasIndex(e => e.WorkspaceId);
            entity.HasIndex(e => e.ProjectId);
            entity.HasIndex(e => e.ParentTaskId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.DueDate);
        });

        modelBuilder.Entity<TaskAssignee>(entity =>
        {
            entity.ToTable("TaskAssignees");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TaskId, e.UserId }).IsUnique();
            entity.HasOne<TaskItem>()
                .WithMany(t => t.Assignees)
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TaskActivityLog>(entity =>
        {
            entity.ToTable("TaskActivityLogs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.TaskId);
        });
    }
}
