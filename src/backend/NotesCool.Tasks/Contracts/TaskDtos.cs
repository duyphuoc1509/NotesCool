using NotesCool.Tasks.Domain;

namespace NotesCool.Tasks.Contracts;

public sealed record CreateTaskRequest(string Title, string? Description, TaskItemPriority Priority = TaskItemPriority.Medium, DateTimeOffset? DueAt = null);
public sealed record UpdateTaskRequest(string Title, string? Description, TaskItemPriority Priority = TaskItemPriority.Medium, DateTimeOffset? DueAt = null);
public sealed record ChangeTaskStatusRequest(TaskItemStatus Status);
public sealed record TaskResponse(Guid Id, string Title, string Description, TaskItemStatus Status, TaskItemPriority Priority, DateTimeOffset? DueAt, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
