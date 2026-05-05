using System.ComponentModel.DataAnnotations;
using NotesCool.Shared.Common;

namespace NotesCool.Tasks.Domain.Entities;

public class TaskActivityLog : Entity
{
    public Guid TaskId { get; private set; }

    [MaxLength(100)]
    public string Action { get; private set; } = string.Empty;

    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }

    protected TaskActivityLog() { }

    public TaskActivityLog(Guid taskId, string action, string? oldValue, string? newValue, string createdBy)
    {
        TaskId = taskId;
        Action = action;
        OldValue = oldValue;
        NewValue = newValue;
        OwnerId = createdBy;
    }
}
