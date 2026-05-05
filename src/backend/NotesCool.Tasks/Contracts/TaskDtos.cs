using System.ComponentModel.DataAnnotations;
using NotesCool.Tasks.Domain.Enums;
using TaskStatus = NotesCool.Tasks.Domain.Enums.TaskStatus;

namespace NotesCool.Tasks.Contracts;

public sealed record TaskDto(
    Guid Id,
    Guid WorkspaceId,
    Guid ProjectId,
    Guid? ParentTaskId,
    string Title,
    string? Description,
    bool IsFavorite,
    TaskStatus Status,
    TaskPriority Priority,
    DateTimeOffset? DueDate,
    int SortOrder,
    string OwnerId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyList<TaskAssigneeDto> Assignees,
    SubTaskProgressDto SubTaskProgress);

public sealed record TaskAssigneeDto(
    string UserId,
    string? AssignedBy,
    DateTimeOffset AssignedAt);

public sealed record TaskActivityLogDto(
    Guid Id,
    Guid TaskId,
    string Action,
    string? OldValue,
    string? NewValue,
    string CreatedBy,
    DateTimeOffset CreatedAt);

public sealed record SubTaskProgressDto(int Total, int Done);

public sealed record SetTaskFavoriteRequest(bool IsFavorite);

public sealed record CreateTaskRequest(
    [Required(ErrorMessage = "Title is required")]
    [StringLength(250, ErrorMessage = "Title must not exceed 250 characters")]
    string Title,
    string? Description,
    TaskPriority Priority = TaskPriority.Medium,
    DateTimeOffset? DueDate = null,
    IReadOnlyList<string>? AssigneeUserIds = null);

public sealed record UpdateTaskRequest(
    [Required(ErrorMessage = "Title is required")]
    [StringLength(250, ErrorMessage = "Title must not exceed 250 characters")]
    string Title,
    string? Description,
    TaskPriority Priority,
    DateTimeOffset? DueDate);

public sealed record ChangeTaskStatusRequest(
    [Required(ErrorMessage = "Status is required")]
    [EnumDataType(typeof(TaskStatus), ErrorMessage = "Invalid status value")]
    TaskStatus Status);

public sealed record ChangeTaskPriorityRequest(
    [Required(ErrorMessage = "Priority is required")]
    [EnumDataType(typeof(TaskPriority), ErrorMessage = "Invalid priority value")]
    TaskPriority Priority);

public sealed record AddTaskAssigneeRequest(
    [Required(ErrorMessage = "UserId is required")]
    string UserId);
