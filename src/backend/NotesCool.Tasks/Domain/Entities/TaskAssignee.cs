using NotesCool.Shared.Common;

namespace NotesCool.Tasks.Domain.Entities;

public class TaskAssignee : Entity
{
    public Guid TaskId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string? AssignedBy { get; private set; }
    public DateTimeOffset AssignedAt { get; private set; } = DateTimeOffset.UtcNow;
    public bool IsActive { get; private set; }

    protected TaskAssignee() { }

    public TaskAssignee(Guid taskId, string userId, string? assignedBy)
    {
        TaskId = taskId;
        UserId = userId;
        AssignedBy = assignedBy;
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }
}
