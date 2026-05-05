using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NotesCool.Shared.Auth;
using NotesCool.Shared.Common;
using NotesCool.Tasks.Application;
using NotesCool.Tasks.Domain.Enums;
using TaskStatus = NotesCool.Tasks.Domain.Enums.TaskStatus;

namespace NotesCool.Tasks.Contracts;

public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MapTasksEndpoints(this IEndpointRouteBuilder builder)
    {
        var projectTasks = builder.MapGroup("api/projects/{projectId:guid}/tasks")
            .WithTags("Tasks")
            .RequireAuthorization();

        projectTasks.MapGet("", async (
            [FromRoute] Guid projectId,
            [FromServices] TasksService service,
            [FromServices] ICurrentUser currentUser,
            [FromQuery] TaskStatus? status = null,
            [FromQuery] TaskPriority? priority = null,
            [FromQuery] string? assigneeId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default) =>
        {
            var result = await service.GetProjectTasksAsync(projectId, currentUser.UserId, status, priority, assigneeId, page, pageSize, ct);
            return Results.Ok(result);
        })
        .WithSummary("Get tasks by project")
        .Produces<PagedResult<TaskDto>>();

        projectTasks.MapPost("", async (
            [FromRoute] Guid projectId,
            [FromBody] CreateTaskRequest request,
            [FromServices] TasksService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            var result = await service.CreateTaskAsync(projectId, request, currentUser.UserId, ct);
            return Results.Created($"api/tasks/{result.Id}", result);
        })
        .WithSummary("Create task")
        .Produces<TaskDto>(StatusCodes.Status201Created);

        var tasks = builder.MapGroup("api/tasks")
            .WithTags("Tasks")
            .RequireAuthorization();

        tasks.MapGet("{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] TasksService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            var result = await service.GetTaskAsync(id, currentUser.UserId, ct);
            return Results.Ok(result);
        })
        .WithSummary("Get task by ID")
        .Produces<TaskDto>();

        tasks.MapPut("{id:guid}", async (
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
        .Produces<TaskDto>();

        tasks.MapPatch("{id:guid}/status", async (
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
        .Produces<TaskDto>();

        tasks.MapPatch("{id:guid}/priority", async (
            [FromRoute] Guid id,
            [FromBody] ChangeTaskPriorityRequest request,
            [FromServices] TasksService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            var result = await service.ChangeTaskPriorityAsync(id, request, currentUser.UserId, ct);
            return Results.Ok(result);
        })
        .WithSummary("Change task priority")
        .Produces<TaskDto>();

        tasks.MapPatch("{id:guid}/favorite", async (
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
        .Produces<TaskDto>();

        tasks.MapDelete("{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] TasksService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            await service.DeleteTaskAsync(id, currentUser.UserId, ct);
            return Results.NoContent();
        })
        .WithSummary("Delete task")
        .Produces(StatusCodes.Status204NoContent);

        tasks.MapGet("{taskId:guid}/subtasks", async (
            [FromRoute] Guid taskId,
            [FromServices] TasksService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            var result = await service.GetSubTasksAsync(taskId, currentUser.UserId, ct);
            return Results.Ok(result);
        })
        .WithSummary("List subtasks")
        .Produces<List<TaskDto>>();

        tasks.MapPost("{taskId:guid}/subtasks", async (
            [FromRoute] Guid taskId,
            [FromBody] CreateTaskRequest request,
            [FromServices] TasksService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            var result = await service.CreateSubTaskAsync(taskId, request, currentUser.UserId, ct);
            return Results.Created($"api/tasks/{result.Id}", result);
        })
        .WithSummary("Create subtask")
        .Produces<TaskDto>(StatusCodes.Status201Created);

        tasks.MapPost("{taskId:guid}/assignees", async (
            [FromRoute] Guid taskId,
            [FromBody] AddTaskAssigneeRequest request,
            [FromServices] TasksService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            await service.AssignUserAsync(taskId, request.UserId, currentUser.UserId, ct);
            return Results.Ok();
        })
        .WithSummary("Assign user to task")
        .Produces(StatusCodes.Status200OK);

        tasks.MapDelete("{taskId:guid}/assignees/{userId}", async (
            [FromRoute] Guid taskId,
            [FromRoute] string userId,
            [FromServices] TasksService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            await service.RemoveAssigneeAsync(taskId, userId, currentUser.UserId, ct);
            return Results.NoContent();
        })
        .WithSummary("Remove assignee from task")
        .Produces(StatusCodes.Status204NoContent);

        tasks.MapGet("{taskId:guid}/activity-logs", async (
            [FromRoute] Guid taskId,
            [FromServices] TasksService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct = default) =>
        {
            var result = await service.GetActivityLogsAsync(taskId, currentUser.UserId, ct);
            return Results.Ok(result);
        })
        .WithSummary("Get task activity logs")
        .Produces<List<TaskActivityLogDto>>();

        return builder;
    }
}
