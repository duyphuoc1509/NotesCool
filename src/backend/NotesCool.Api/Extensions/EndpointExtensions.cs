using NotesCool.Notes.Application;
using NotesCool.Notes.Contracts;
using NotesCool.Shared.Auth;
using NotesCool.Shared.Common;
using NotesCool.Tasks.Application;
using NotesCool.Tasks.Contracts;
using NotesCool.Tasks.Domain;

namespace NotesCool.Api.Extensions;

public static class EndpointExtensions
{
    public static void MapNotesEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/notes").RequireAuthorization();
        g.MapGet("/", async (ICurrentUser user, NotesService s, string? query, int page = 1, int pageSize = 20, CancellationToken ct = default) => Results.Ok(await s.SearchAsync(user.UserId, query, new(page, pageSize), ct)));
        g.MapGet("/{id:guid}", async (ICurrentUser user, NotesService s, Guid id, CancellationToken ct) => Results.Ok(await s.GetAsync(user.UserId, id, ct)));
        g.MapPost("/", async (ICurrentUser user, NotesService s, CreateNoteRequest r, CancellationToken ct) => Results.Ok(await s.CreateAsync(user.UserId, r, ct)));
        g.MapPut("/{id:guid}", async (ICurrentUser user, NotesService s, Guid id, UpdateNoteRequest r, CancellationToken ct) => Results.Ok(await s.UpdateAsync(user.UserId, id, r, ct)));
        g.MapDelete("/{id:guid}", async (ICurrentUser user, NotesService s, Guid id, CancellationToken ct) => { await s.ArchiveAsync(user.UserId, id, ct); return Results.NoContent(); });
    }
    public static void MapTasksEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/tasks").RequireAuthorization();
        g.MapGet("/", async (ICurrentUser user, TasksService s, TaskItemStatus? status, string? sort, int page = 1, int pageSize = 20, CancellationToken ct = default) => Results.Ok(await s.ListAsync(user.UserId, status, sort, new(page, pageSize), ct)));
        g.MapGet("/{id:guid}", async (ICurrentUser user, TasksService s, Guid id, CancellationToken ct) => Results.Ok(await s.GetAsync(user.UserId, id, ct)));
        g.MapPost("/", async (ICurrentUser user, TasksService s, CreateTaskRequest r, CancellationToken ct) => Results.Ok(await s.CreateAsync(user.UserId, r, ct)));
        g.MapPut("/{id:guid}", async (ICurrentUser user, TasksService s, Guid id, UpdateTaskRequest r, CancellationToken ct) => Results.Ok(await s.UpdateAsync(user.UserId, id, r, ct)));
        g.MapPatch("/{id:guid}/status", async (ICurrentUser user, TasksService s, Guid id, ChangeTaskStatusRequest r, CancellationToken ct) => Results.Ok(await s.ChangeStatusAsync(user.UserId, id, r, ct)));
        g.MapDelete("/{id:guid}", async (ICurrentUser user, TasksService s, Guid id, CancellationToken ct) => { await s.ArchiveAsync(user.UserId, id, ct); return Results.NoContent(); });
    }
}
