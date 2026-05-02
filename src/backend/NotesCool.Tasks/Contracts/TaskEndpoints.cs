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
        })
        .WithSummary("Get tasks")
        .WithDescription("Returns a paged result of tasks for the current user, optionally filtered by status.")
        .Produces<PagedResult<TaskDto>>();

        group.MapGet("{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] TasksService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            var result = await service.GetTaskAsync(id, currentUser.UserId, ct);
            return Results.Ok(result);
        })
        .WithSummary("Get task by ID")
        .WithDescription("Returns a single task for the current user.")
        .Produces<TaskDto>();

        group.MapPost("", async (
            [FromBody] CreateTaskRequest request,
            [FromServices] TasksService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            var result = await service.CreateTaskAsync(request, currentUser.UserId, ct);
            return Results.Created($"api/tasks/{result.Id}", result);
        })
        .WithSummary("Create task")
        .WithDescription("Creates a new task for the current user.")
        .Produces<TaskDto>(StatusCodes.Status201Created);

        group.MapPut("{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateTaskRequest request,
            [FromServices] TasksService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            var result = await service.UpdateTaskAsync(id, request, currentUser.UserId, ct);
            return Results.Ok(result);
        })
        .WithSummary("Update task")
        .WithDescription("Updates a task's title, description, and due date.")
        .Produces<TaskDto>();

        group.MapPatch("{id:guid}/status", async (
            [FromRoute] Guid id,
            [FromBody] ChangeTaskStatusRequest request,
            [FromServices] TasksService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            var result = await service.ChangeTaskStatusAsync(id, request, currentUser.UserId, ct);
            return Results.Ok(result);
        })
        .WithSummary("Change task status")
        .WithDescription("Updates the status of a task.")
        .Produces<TaskDto>();

        group.MapPatch("{id:guid}/favorite", async (
            [FromRoute] Guid id,
            [FromBody] SetTaskFavoriteRequest request,
            [FromServices] TasksService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            var result = await service.SetTaskFavoriteAsync(id, request.IsFavorite, currentUser.UserId, ct);
            return Results.Ok(result);
        })
        .WithSummary("Set task favorite")
        .WithDescription("Updates favorite state for a task.")
        .Produces<TaskDto>();

        group.MapDelete("{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] TasksService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            await service.DeleteTaskAsync(id, currentUser.UserId, ct);
            return Results.NoContent();
        })
        .WithSummary("Delete task")
        .WithDescription("Deletes a task.")
        .Produces(StatusCodes.Status204NoContent);

        return builder;
    }
}
