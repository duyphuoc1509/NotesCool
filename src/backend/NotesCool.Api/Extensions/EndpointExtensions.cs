using Microsoft.AspNetCore.Http;
using NotesCool.Notes.Application;
using NotesCool.Notes.Contracts;
using NotesCool.Shared.Auth;
using NotesCool.Shared.Common;

namespace NotesCool.Api.Extensions;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder builder)
    {
        var notesGroup = builder.MapGroup("api/notes").WithTags("Notes").RequireAuthorization();
        notesGroup.MapGet("", async (string? query, int? page, int? pageSize, NotesService service, ICurrentUser u, CancellationToken ct) => Results.Ok(await service.SearchAsync(u.UserId, query, new PageRequest(page ?? 1, pageSize ?? 20), ct)))
            .WithSummary("Search notes")
            .WithDescription("Returns the current user's notes with optional text search and pagination.")
            .Produces<PagedResult<NoteResponse>>();

        notesGroup.MapGet("{id:guid}", async (Guid id, NotesService service, ICurrentUser u, CancellationToken ct) => Results.Ok(await service.GetAsync(u.UserId, id, ct)))
            .WithSummary("Get note by ID")
            .WithDescription("Returns a single note owned by the current user.")
            .Produces<NoteResponse>();

        notesGroup.MapPost("", async (CreateNoteRequest req, NotesService service, ICurrentUser u, CancellationToken ct) => { var r = await service.CreateAsync(u.UserId, req, ct); return Results.Created($"api/notes/{r.Id}", r); })
            .WithSummary("Create note")
            .WithDescription("Creates a new note for the current user.")
            .Produces<NoteResponse>(StatusCodes.Status201Created);

        notesGroup.MapPut("{id:guid}", async (Guid id, UpdateNoteRequest req, NotesService service, ICurrentUser u, CancellationToken ct) => Results.Ok(await service.UpdateAsync(u.UserId, id, req, ct)))
            .WithSummary("Update note")
            .WithDescription("Updates title and content for a note owned by the current user.")
            .Produces<NoteResponse>();

        notesGroup.MapDelete("{id:guid}", async (Guid id, NotesService service, ICurrentUser u, CancellationToken ct) => { await service.ArchiveAsync(u.UserId, id, ct); return Results.NoContent(); })
            .WithSummary("Archive note")
            .WithDescription("Archives a note owned by the current user.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }
}
