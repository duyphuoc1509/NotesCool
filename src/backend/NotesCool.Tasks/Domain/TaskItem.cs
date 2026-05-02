using System.ComponentModel.DataAnnotations;
using NotesCool.Shared.Common;

namespace NotesCool.Tasks.Domain;

public enum TaskStatus
{
    Todo = 0,
    InProgress = 1,
    Done = 2,
    // Value 3 was previously named Cancelled. Keep the value stable for existing rows,
    // but expose the frontend-aligned name used by Kanban filters/actions.
    Blocked = 3,
    InReview = 4,
    Archived = 5
}

public class TaskItem : Entity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; private set; } = string.Empty;
    
    public string? Description { get; private set; }
    
    public TaskStatus Status { get; private set; } = TaskStatus.Todo;
    
    public bool IsFavorite { get; private set; }
    
    public DateTimeOffset? DueDate { get; private set; }
    
    protected TaskItem() { } // For EF Core
    
    public TaskItem(string title, string? description, DateTimeOffset? dueDate, string ownerId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));
            
        Title = title;
        Description = description;
        DueDate = dueDate;
        OwnerId = ownerId;
    }

    public void Update(string title, string? description, DateTimeOffset? dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));

        Title = title;
        Description = description;
        DueDate = dueDate;
        Touch();
    }
    
    public void ChangeStatus(TaskStatus newStatus)
    {
        if (!Enum.IsDefined(typeof(TaskStatus), newStatus))
            throw new ArgumentException("Invalid task status", nameof(newStatus));

        if (Status != newStatus)
        {
            Status = newStatus;
            Touch();
        }
    }

    public void SetFavorite(bool isFavorite)
    {
        if (IsFavorite == isFavorite)
            return;

        IsFavorite = isFavorite;
        Touch();
    }
}
