using System.ComponentModel.DataAnnotations;
using NotesCool.Tasks.Domain;
using TaskStatus = NotesCool.Tasks.Domain.TaskStatus;

namespace NotesCool.Tasks.Contracts;

public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    TaskStatus Status,
    DateTimeOffset? DueDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public record CreateTaskRequest(
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title must not exceed 200 characters")]
    string Title,
    string? Description,
    DateTimeOffset? DueDate
);

public record UpdateTaskRequest(
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title must not exceed 200 characters")]
    string Title,
    string? Description,
    DateTimeOffset? DueDate
);

public record ChangeTaskStatusRequest(
    [Required(ErrorMessage = "Status is required")]
    [EnumDataType(typeof(TaskStatus), ErrorMessage = "Invalid status value")]
    TaskStatus Status
);
