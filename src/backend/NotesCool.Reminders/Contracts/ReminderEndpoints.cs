using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NotesCool.Reminders.Application;
using NotesCool.Shared.Auth;

namespace NotesCool.Reminders.Contracts;

public static class ReminderEndpoints
{
    public static IEndpointRouteBuilder MapReminderEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/tasks/{taskId:guid}/reminders")
            .WithTags("Reminders")
            .RequireAuthorization();

        group.MapGet("", async (
            [FromRoute] Guid taskId,
            [FromServices] ReminderService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            var result = await service.GetTaskRemindersAsync(taskId, currentUser.UserId, ct);
            return Results.Ok(result);
        })
        .WithSummary("Get task reminders")
        .Produces<IReadOnlyList<ReminderDto>>();

        group.MapPut("", async (
            [FromRoute] Guid taskId,
            [FromBody] UpdateTaskRemindersRequest request,
            [FromServices] ReminderService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            var result = await service.UpsertTaskRemindersAsync(taskId, currentUser.UserId, request.TaskTitle, request.DueDateUtc, request.Reminders, ct);
            return Results.Ok(result);
        })
        .WithSummary("Create or update task reminders")
        .Produces<IReadOnlyList<ReminderDto>>();

        group.MapDelete("", async (
            [FromRoute] Guid taskId,
            [FromServices] ReminderService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            await service.CancelTaskRemindersAsync(taskId, currentUser.UserId, ct);
            return Results.NoContent();
        })
        .WithSummary("Cancel task reminders")
        .Produces(StatusCodes.Status204NoContent);

        group.MapPost("{reminderId:guid}/device-sync", async (
            [FromRoute] Guid reminderId,
            [FromBody] ReportDeviceSyncRequest request,
            [FromServices] ReminderService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            var result = await service.ReportDeviceSyncAsync(reminderId, currentUser.UserId, request, ct);
            return Results.Ok(result);
        })
        .WithSummary("Report device calendar sync result")
        .Produces<ReportDeviceSyncResponse>();

        return builder;
    }
}
