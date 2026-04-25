using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NotesCool.Shared.Auth;
using NotesCool.Shared.Common;
using NotesCool.Tasks.Application;
using TaskStatus = NotesCool.Tasks.Domain.TaskStatus;

namespace NotesCool.Tasks.Contracts;

public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MapTasksEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/tasks")
            .WithTags("Tasks")
            .RequireAuthorization();

        group.MapGet("", async (
            [FromServices] TasksService service,
            [FromServices] ICurrentUser currentUser,
            [FromQuery] TaskStatus? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default) =>
        {
            var result = await service.GetTasksAsync(currentUser.UserId, status, page, pageSize, ct);
            return Results.Ok(result);
        });

        group.MapGet("{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] TasksService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            var result = await service.GetTaskAsync(id, currentUser.UserId, ct);
            return Results.Ok(result);
        });

        group.MapPost("", async (
            [FromBody] CreateTaskRequest request,
            [FromServices] TasksService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            var result = await service.CreateTaskAsync(request, currentUser.UserId, ct);
            return Results.Created($"api/tasks/{result.Id}", result);
        });

        group.MapPut("{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateTaskRequest request,
            [FromServices] TasksService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            var result = await service.UpdateTaskAsync(id, request, currentUser.UserId, ct);
            return Results.Ok(result);
        });

        group.MapPatch("{id:guid}/status", async (
            [FromRoute] Guid id,
            [FromBody] ChangeTaskStatusRequest request,
            [FromServices] TasksService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            var result = await service.ChangeTaskStatusAsync(id, request, currentUser.UserId, ct);
            return Results.Ok(result);
        });

        group.MapDelete("{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] TasksService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            await service.DeleteTaskAsync(id, currentUser.UserId, ct);
            return Results.NoContent();
        });

        return builder;
    }
}
