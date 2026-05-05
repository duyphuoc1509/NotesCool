using System.ComponentModel.DataAnnotations;
using NotesCool.Shared.Common;
using NotesCool.Tasks.Domain.Enums;
using TaskStatus = NotesCool.Tasks.Domain.Enums.TaskStatus;

namespace NotesCool.Tasks.Domain.Entities;

public class TaskItem : Entity
{
    public Guid WorkspaceId { get; private set; }
    public Guid ProjectId { get; private set; }
    public Guid? ParentTaskId { get; private set; }

    [Required]
    [MaxLength(250)]
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public TaskStatus Status { get; private set; } = TaskStatus.Todo;
    public TaskPriority Priority { get; private set; } = TaskPriority.Medium;
    public DateTimeOffset? DueDate { get; private set; }
    public int SortOrder { get; private set; }
    public string? DeletedBy { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public bool IsDeleted => DeletedAt.HasValue;
    public string? UpdatedBy { get; private set; }

    // Navigation
    public ICollection<TaskAssignee> Assignees { get; private set; } = new List<TaskAssignee>();
    public ICollection<TaskActivityLog> ActivityLogs { get; private set; } = new List<TaskActivityLog>();

    protected TaskItem() { }

    public TaskItem(Guid workspaceId, Guid projectId, string title, string? description,
        TaskPriority priority, DateTimeOffset? dueDate, string ownerId, Guid? parentTaskId = null)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title required", nameof(title));
        if (title.Length > 250) throw new ArgumentException("Title too long", nameof(title));
        WorkspaceId = workspaceId;
        ProjectId = projectId;
        ParentTaskId = parentTaskId;
        Title = title;
        Description = description;
        Priority = priority;
        DueDate = dueDate;
        OwnerId = ownerId;
    }

    public void Update(string title, string? description, TaskPriority priority, DateTimeOffset? dueDate, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title required", nameof(title));
        if (title.Length > 250) throw new ArgumentException("Title too long", nameof(title));
        Title = title;
        Description = description;
        Priority = priority;
        DueDate = dueDate;
        UpdatedBy = updatedBy;
        Touch();
    }

    public void ChangeStatus(TaskStatus status, string updatedBy)
    {
        Status = status;
        UpdatedBy = updatedBy;
        Touch();
    }

    public void SoftDelete(string deletedBy)
    {
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
        Touch();
    }
}
