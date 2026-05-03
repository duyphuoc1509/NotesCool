using System.ComponentModel.DataAnnotations;
using NotesCool.Reminders.Domain;

namespace NotesCool.Reminders.Contracts;

public record ReminderDto(
    Guid Id,
    Guid TaskId,
    string AccountId,
    string TaskTitle,
    DateTimeOffset DueDateUtc,
    int OffsetMinutes,
    DateTimeOffset ReminderTimeUtc,
    ReminderStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public record ReminderConfigDto(
    [Required] int OffsetMinutes
);

public record CreateTaskRemindersRequest(
    [Required] string TaskTitle,
    [Required] DateTimeOffset DueDateUtc,
    [Required] List<ReminderConfigDto> Reminders
);

public record UpdateTaskRemindersRequest(
    [Required] string TaskTitle,
    [Required] DateTimeOffset DueDateUtc,
    [Required] List<ReminderConfigDto> Reminders
);

public record ReportDeviceSyncRequest(
    [Required] string DeviceId,
    [Required] string Platform,
    [Required] bool Success,
    string? NativeCalendarEventId,
    string? FailureReason
);

public record ReportDeviceSyncResponse(
    Guid SyncId,
    Guid ReminderId,
    string DeviceId,
    string Platform,
    string NativeCalendarEventId,
    ReminderDeviceSyncStatus Status
);
