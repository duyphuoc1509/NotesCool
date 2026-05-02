using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NotesCool.Reminders.Application;
using NotesCool.Reminders.Contracts;
using NotesCool.Reminders.Domain;
using NotesCool.Reminders.Infrastructure;
using NotesCool.Shared.Errors;

namespace NotesCool.Tasks.Tests;

public class ReminderServiceTests
{
    [Fact]
    public async Task UpsertTaskRemindersAsync_ShouldCreateDistinctReminders()
    {
        await using var dbContext = CreateDbContext();
        var service = new ReminderService(dbContext);
        var taskId = Guid.NewGuid();
        var dueDate = new DateTimeOffset(2026, 5, 10, 10, 0, 0, TimeSpan.Zero);

        var result = await service.UpsertTaskRemindersAsync(
            taskId,
            "account-1",
            "Prepare release",
            dueDate,
            [new ReminderConfigDto(60), new ReminderConfigDto(15), new ReminderConfigDto(60)]);

        result.Should().HaveCount(2);
        result.Select(x => x.OffsetMinutes).Should().BeEquivalentTo([60, 15]);
        result.Should().OnlyContain(x => x.Status == ReminderStatus.Pending);
    }

    [Fact]
    public async Task UpsertTaskRemindersAsync_ShouldCancelRemovedOffsets()
    {
        await using var dbContext = CreateDbContext();
        var service = new ReminderService(dbContext);
        var taskId = Guid.NewGuid();
        var dueDate = new DateTimeOffset(2026, 5, 10, 10, 0, 0, TimeSpan.Zero);

        await service.UpsertTaskRemindersAsync(
            taskId,
            "account-1",
            "Prepare release",
            dueDate,
            [new ReminderConfigDto(60), new ReminderConfigDto(15)]);

        var result = await service.UpsertTaskRemindersAsync(
            taskId,
            "account-1",
            "Prepare release",
            dueDate,
            [new ReminderConfigDto(15)]);

        result.Should().ContainSingle(x => x.OffsetMinutes == 15 && x.Status == ReminderStatus.Pending);

        var canceled = await dbContext.Reminders.IgnoreQueryFilters()
            .SingleAsync(x => x.TaskId == taskId && x.OffsetMinutes == 60);
        canceled.Status.Should().Be(ReminderStatus.Canceled);
    }

    [Fact]
    public async Task CancelTaskRemindersAsync_ShouldCancelRemindersAndDeviceSyncs()
    {
        await using var dbContext = CreateDbContext();
        var service = new ReminderService(dbContext);
        var taskId = Guid.NewGuid();
        var dueDate = new DateTimeOffset(2026, 5, 10, 10, 0, 0, TimeSpan.Zero);

        var reminders = await service.UpsertTaskRemindersAsync(
            taskId,
            "account-1",
            "Prepare release",
            dueDate,
            [new ReminderConfigDto(15)]);

        await service.ReportDeviceSyncAsync(
            reminders[0].Id,
            "account-1",
            new ReportDeviceSyncRequest("device-1", "ios", true, "event-1", null));

        await service.CancelTaskRemindersAsync(taskId, "account-1");

        var reminder = await dbContext.Reminders.IgnoreQueryFilters().SingleAsync();
        var sync = await dbContext.DeviceSyncs.IgnoreQueryFilters().SingleAsync();

        reminder.Status.Should().Be(ReminderStatus.Canceled);
        sync.Status.Should().Be(ReminderDeviceSyncStatus.Canceled);
    }

    [Fact]
    public async Task ReportDeviceSyncAsync_ShouldMarkReminderSynced()
    {
        await using var dbContext = CreateDbContext();
        var service = new ReminderService(dbContext);
        var taskId = Guid.NewGuid();
        var dueDate = new DateTimeOffset(2026, 5, 10, 10, 0, 0, TimeSpan.Zero);

        var reminders = await service.UpsertTaskRemindersAsync(
            taskId,
            "account-1",
            "Prepare release",
            dueDate,
            [new ReminderConfigDto(15)]);

        var response = await service.ReportDeviceSyncAsync(
            reminders[0].Id,
            "account-1",
            new ReportDeviceSyncRequest("device-1", "android", true, "native-event-1", null));

        response.Status.Should().Be(ReminderDeviceSyncStatus.Synced);

        var reminder = await dbContext.Reminders.SingleAsync();
        reminder.Status.Should().Be(ReminderStatus.Synced);
    }

    [Fact]
    public async Task UpsertTaskRemindersAsync_ShouldRejectMissingDueDate()
    {
        await using var dbContext = CreateDbContext();
        var service = new ReminderService(dbContext);

        var act = async () => await service.UpsertTaskRemindersAsync(
            Guid.NewGuid(),
            "account-1",
            "Prepare release",
            default,
            [new ReminderConfigDto(15)]);

        var exception = await act.Should().ThrowAsync<ApiException>();
        exception.Which.Code.Should().Be("due_date_required");
    }

    private static RemindersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<RemindersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RemindersDbContext(options);
    }
}
