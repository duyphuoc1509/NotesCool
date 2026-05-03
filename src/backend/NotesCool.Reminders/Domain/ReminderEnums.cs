namespace NotesCool.Reminders.Domain;

public enum ReminderStatus
{
    Pending = 0,
    Synced = 1,
    Failed = 2,
    Canceled = 3
}

public enum ReminderDeviceSyncStatus
{
    Pending = 0,
    Synced = 1,
    Failed = 2,
    Canceled = 3
}
