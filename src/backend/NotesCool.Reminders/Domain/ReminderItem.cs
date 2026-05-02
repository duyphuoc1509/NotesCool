using System.ComponentModel.DataAnnotations;
using NotesCool.Shared.Common;

namespace NotesCool.Reminders.Domain;

public class ReminderItem : Entity
{
    [Required]
    public Guid TaskId { get; private set; }

    [Required]
    [MaxLength(100)]
    public string AccountId { get; private set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string TaskTitle { get; private set; } = string.Empty;

    [Required]
    public DateTimeOffset DueDateUtc { get; private set; }

    [Required]
    [MaxLength(50)]
    public string DueDateSourceTimezone { get; private set; } = Timezones.UtcPlus7;

    public int OffsetMinutes { get; private set; }

    [Required]
    public DateTimeOffset ReminderTimeUtc { get; private set; }

    public ReminderStatus Status { get; private set; } = ReminderStatus.Pending;

    protected ReminderItem()
    {
    }

    public ReminderItem(
        Guid taskId,
        string accountId,
        string taskTitle,
        DateTimeOffset dueDateUtc,
        int offsetMinutes,
        DateTimeOffset reminderTimeUtc,
        string ownerId)
    {
        if (taskId == Guid.Empty) throw new ArgumentException("Task is required", nameof(taskId));
        if (string.IsNullOrWhiteSpace(accountId)) throw new ArgumentException("Account is required", nameof(accountId));
        if (string.IsNullOrWhiteSpace(taskTitle)) throw new ArgumentException("Task title is required", nameof(taskTitle));
        if (string.IsNullOrWhiteSpace(ownerId)) throw new ArgumentException("Owner is required", nameof(ownerId));

        TaskId = taskId;
        AccountId = accountId;
        TaskTitle = taskTitle;
        DueDateUtc = dueDateUtc;
        OffsetMinutes = offsetMinutes;
        ReminderTimeUtc = reminderTimeUtc;
        OwnerId = ownerId;
    }

    public string GetIdempotencyKey()
        => $"{AccountId}:{TaskId}:{OffsetMinutes}";

    public void Reschedule(string taskTitle, DateTimeOffset dueDateUtc, int offsetMinutes, DateTimeOffset reminderTimeUtc)
    {
        if (string.IsNullOrWhiteSpace(taskTitle)) throw new ArgumentException("Task title is required", nameof(taskTitle));

        TaskTitle = taskTitle;
        DueDateUtc = dueDateUtc;
        OffsetMinutes = offsetMinutes;
        ReminderTimeUtc = reminderTimeUtc;
        Status = ReminderStatus.Pending;
        Touch();
    }

    public void MarkSynced()
    {
        Status = ReminderStatus.Synced;
        Touch();
    }

    public void MarkFailed()
    {
        Status = ReminderStatus.Failed;
        Touch();
    }

    public void Cancel()
    {
        if (Status == ReminderStatus.Canceled)
        {
            return;
        }

        Status = ReminderStatus.Canceled;
        Touch();
    }
}

public static class Timezones
{
    public const string UtcPlus7 = "UTC+7";
}
