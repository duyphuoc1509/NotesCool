using NotesCool.Shared.Common;
using NotesCool.Shared.Errors;

namespace NotesCool.Tasks.Domain;

public enum TaskItemStatus { Todo = 0, InProgress = 1, Done = 2 }
public enum TaskItemPriority { Low = 0, Medium = 1, High = 2 }

public sealed class TaskItem : Entity
{
    private TaskItem() { }
    public TaskItem(string ownerId, string title, string? description, TaskItemPriority priority = TaskItemPriority.Medium)
    {
        OwnerId = string.IsNullOrWhiteSpace(ownerId) ? throw new ApiException("owner_required", "Owner is required.") : ownerId;
        Priority = priority; Update(title, description);
    }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TaskItemStatus Status { get; private set; } = TaskItemStatus.Todo;
    public TaskItemPriority Priority { get; private set; } = TaskItemPriority.Medium;
    public DateTimeOffset? DueAt { get; private set; }
    public void Update(string title, string? description, DateTimeOffset? dueAt = null, TaskItemPriority? priority = null)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ApiException("task_title_required", "Task title is required.");
        if (title.Length > 200) throw new ApiException("task_title_too_long", "Task title must be 200 characters or fewer.");
        Title = title.Trim(); Description = description?.Trim() ?? string.Empty; DueAt = dueAt; if (priority.HasValue) Priority = priority.Value; Touch();
    }
    public void ChangeStatus(TaskItemStatus next)
    {
        if (Status == TaskItemStatus.Done && next != TaskItemStatus.Done) throw new ApiException("invalid_task_transition", "Done tasks cannot be reopened in MVP V1.");
        Status = next; Touch();
    }
}
