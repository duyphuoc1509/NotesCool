using Microsoft.EntityFrameworkCore;
using NotesCool.Reminders.Contracts;
using NotesCool.Reminders.Domain;
using NotesCool.Reminders.Infrastructure;
using NotesCool.Shared.Errors;

namespace NotesCool.Reminders.Application;

public class ReminderService
{
    private static readonly TimeSpan SourceTimezoneOffset = TimeSpan.FromHours(7);
    private readonly RemindersDbContext _dbContext;

    public ReminderService(RemindersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ReminderDto>> GetTaskRemindersAsync(Guid taskId, string accountId, CancellationToken cancellationToken = default)
    {
        var reminders = await _dbContext.Reminders
            .AsNoTracking()
            .Where(r => r.TaskId == taskId && r.AccountId == accountId)
            .OrderBy(r => r.ReminderTimeUtc)
            .ToListAsync(cancellationToken);

        return reminders.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<ReminderDto>> UpsertTaskRemindersAsync(
        Guid taskId,
        string accountId,
        string taskTitle,
        DateTimeOffset dueDateUtc,
        IReadOnlyCollection<ReminderConfigDto> reminderConfigs,
        CancellationToken cancellationToken = default)
    {
        ValidateReminderRequest(taskId, accountId, taskTitle, dueDateUtc, reminderConfigs);

        var normalizedDueDateUtc = NormalizeFromUtcPlus7Source(dueDateUtc);
        var distinctOffsets = reminderConfigs
            .Select(r => r.OffsetMinutes)
            .Distinct()
            .OrderByDescending(offset => offset)
            .ToArray();

        var existingReminders = await _dbContext.Reminders
            .IgnoreQueryFilters()
            .Where(r => r.TaskId == taskId && r.AccountId == accountId)
            .ToListAsync(cancellationToken);

        foreach (var offset in distinctOffsets)
        {
            var reminderTimeUtc = normalizedDueDateUtc.AddMinutes(-offset);
            var reminder = existingReminders.FirstOrDefault(r => r.OffsetMinutes == offset);

            if (reminder is null)
            {
                reminder = new ReminderItem(
                    taskId,
                    accountId,
                    taskTitle,
                    normalizedDueDateUtc,
                    offset,
                    reminderTimeUtc,
                    accountId);
                _dbContext.Reminders.Add(reminder);
                existingReminders.Add(reminder);
            }
            else
            {
                reminder.Reschedule(taskTitle, normalizedDueDateUtc, offset, reminderTimeUtc);
            }
        }

        var removedReminders = existingReminders
            .Where(r => !distinctOffsets.Contains(r.OffsetMinutes) && r.Status != ReminderStatus.Canceled)
            .ToList();

        foreach (var reminder in removedReminders)
        {
            reminder.Cancel();
            await CancelDeviceSyncsAsync(reminder.Id, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetTaskRemindersAsync(taskId, accountId, cancellationToken);
    }

    public async Task CancelTaskRemindersAsync(Guid taskId, string accountId, CancellationToken cancellationToken = default)
    {
        var reminders = await _dbContext.Reminders
            .IgnoreQueryFilters()
            .Where(r => r.TaskId == taskId && r.AccountId == accountId && r.Status != ReminderStatus.Canceled)
            .ToListAsync(cancellationToken);

        foreach (var reminder in reminders)
        {
            reminder.Cancel();
            await CancelDeviceSyncsAsync(reminder.Id, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ReportDeviceSyncResponse> ReportDeviceSyncAsync(
        Guid reminderId,
        string accountId,
        ReportDeviceSyncRequest request,
        CancellationToken cancellationToken = default)
    {
        var reminder = await _dbContext.Reminders
            .FirstOrDefaultAsync(r => r.Id == reminderId && r.AccountId == accountId, cancellationToken);

        if (reminder is null)
        {
            throw new ApiException("reminder_not_found", "Reminder not found", 404);
        }

        if (string.IsNullOrWhiteSpace(request.DeviceId))
        {
            throw new ApiException("device_required", "Device is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.Platform))
        {
            throw new ApiException("platform_required", "Platform is required.", 400);
        }

        var nativeEventId = request.NativeCalendarEventId ?? string.Empty;
        var sync = await _dbContext.DeviceSyncs
            .FirstOrDefaultAsync(s => s.ReminderItemId == reminderId && s.DeviceId == request.DeviceId, cancellationToken);

        if (sync is null)
        {
            sync = new ReminderDeviceSync(
                reminderId,
                request.DeviceId,
                request.Platform,
                nativeEventId.Length == 0 ? $"pending:{reminderId}:{request.DeviceId}" : nativeEventId,
                accountId);
            _dbContext.DeviceSyncs.Add(sync);
        }

        if (request.Success)
        {
            if (string.IsNullOrWhiteSpace(request.NativeCalendarEventId))
            {
                throw new ApiException("native_calendar_event_required", "Native calendar event is required when sync succeeds.", 400);
            }

            sync.MarkSynced(request.NativeCalendarEventId);
            reminder.MarkSynced();
        }
        else
        {
            sync.MarkFailed(string.IsNullOrWhiteSpace(request.FailureReason) ? "sync_failed" : request.FailureReason);
            reminder.MarkFailed();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ReportDeviceSyncResponse(sync.Id, reminder.Id, sync.DeviceId, sync.Platform, sync.NativeCalendarEventId, sync.Status);
    }

    private async Task CancelDeviceSyncsAsync(Guid reminderId, CancellationToken cancellationToken)
    {
        var syncs = await _dbContext.DeviceSyncs
            .Where(s => s.ReminderItemId == reminderId && s.Status != ReminderDeviceSyncStatus.Canceled)
            .ToListAsync(cancellationToken);

        foreach (var sync in syncs)
        {
            sync.Cancel();
        }
    }

    private static void ValidateReminderRequest(
        Guid taskId,
        string accountId,
        string taskTitle,
        DateTimeOffset dueDateUtc,
        IReadOnlyCollection<ReminderConfigDto> reminderConfigs)
    {
        if (taskId == Guid.Empty)
        {
            throw new ApiException("task_required", "Task is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(accountId))
        {
            throw new ApiException("account_required", "Account is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(taskTitle))
        {
            throw new ApiException("task_title_required", "Task title is required.", 400);
        }

        if (dueDateUtc == default)
        {
            throw new ApiException("due_date_required", "Due date is required to create reminders.", 400);
        }

        if (reminderConfigs.Count == 0)
        {
            throw new ApiException("reminders_required", "Reminder list is invalid.", 400);
        }

        foreach (var reminder in reminderConfigs)
        {
            if (reminder.OffsetMinutes < 0)
            {
                throw new ApiException("reminder_time_invalid", "Reminder time is invalid.", 400);
            }

            var reminderTimeUtc = dueDateUtc.AddMinutes(-reminder.OffsetMinutes);
            if (reminderTimeUtc > dueDateUtc)
            {
                throw new ApiException("reminder_after_due_date", "Reminder must not be after due date.", 400);
            }
        }
    }

    private static DateTimeOffset NormalizeFromUtcPlus7Source(DateTimeOffset dueDate)
    {
        var sourceLocal = dueDate.ToOffset(SourceTimezoneOffset);
        return sourceLocal.ToUniversalTime();
    }

    private static ReminderDto MapToDto(ReminderItem reminder)
    {
        return new ReminderDto(
            reminder.Id,
            reminder.TaskId,
            reminder.AccountId,
            reminder.TaskTitle,
            reminder.DueDateUtc,
            reminder.OffsetMinutes,
            reminder.ReminderTimeUtc,
            reminder.Status,
            reminder.CreatedAt,
            reminder.UpdatedAt);
    }
}
