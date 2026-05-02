using System.ComponentModel.DataAnnotations;
using NotesCool.Shared.Common;

namespace NotesCool.Reminders.Domain;

public class ReminderDeviceSync : Entity
{
    [Required]
    public Guid ReminderItemId { get; private set; }

    [Required]
    [MaxLength(100)]
    public string DeviceId { get; private set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Platform { get; private set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string NativeCalendarEventId { get; private set; } = string.Empty;

    public ReminderDeviceSyncStatus Status { get; private set; } = ReminderDeviceSyncStatus.Pending;

    [MaxLength(100)]
    public string? FailureReason { get; private set; }

    protected ReminderDeviceSync()
    {
    }

    public ReminderDeviceSync(
        Guid reminderItemId,
        string deviceId,
        string platform,
        string nativeCalendarEventId,
        string ownerId)
    {
        if (reminderItemId == Guid.Empty) throw new ArgumentException("Reminder is required", nameof(reminderItemId));
        if (string.IsNullOrWhiteSpace(deviceId)) throw new ArgumentException("Device is required", nameof(deviceId));
        if (string.IsNullOrWhiteSpace(platform)) throw new ArgumentException("Platform is required", nameof(platform));
        if (string.IsNullOrWhiteSpace(nativeCalendarEventId)) throw new ArgumentException("Native calendar event is required", nameof(nativeCalendarEventId));
        if (string.IsNullOrWhiteSpace(ownerId)) throw new ArgumentException("Owner is required", nameof(ownerId));

        ReminderItemId = reminderItemId;
        DeviceId = deviceId;
        Platform = platform;
        NativeCalendarEventId = nativeCalendarEventId;
        OwnerId = ownerId;
    }

    public string GetIdempotencyKey()
        => $"{ReminderItemId}:{DeviceId}";

    public void MarkSynced(string nativeCalendarEventId)
    {
        if (string.IsNullOrWhiteSpace(nativeCalendarEventId)) throw new ArgumentException("Native calendar event is required", nameof(nativeCalendarEventId));

        NativeCalendarEventId = nativeCalendarEventId;
        FailureReason = null;
        Status = ReminderDeviceSyncStatus.Synced;
        Touch();
    }

    public void MarkFailed(string failureReason)
    {
        if (string.IsNullOrWhiteSpace(failureReason)) throw new ArgumentException("Failure reason is required", nameof(failureReason));

        FailureReason = failureReason;
        Status = ReminderDeviceSyncStatus.Failed;
        Touch();
    }

    public void Cancel()
    {
        if (Status == ReminderDeviceSyncStatus.Canceled)
        {
            return;
        }

        Status = ReminderDeviceSyncStatus.Canceled;
        Touch();
    }
}
