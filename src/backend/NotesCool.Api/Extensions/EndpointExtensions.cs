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
        notesGroup.MapGet("", async (string? query, int page, int pageSize, NotesService service, ICurrentUser u, CancellationToken ct) => Results.Ok(await service.SearchAsync(u.UserId, query, new PageRequest(page, pageSize), ct)));
        notesGroup.MapGet("{id:guid}", async (Guid id, NotesService service, ICurrentUser u, CancellationToken ct) => Results.Ok(await service.GetAsync(u.UserId, id, ct)));
        notesGroup.MapPost("", async (CreateNoteRequest req, NotesService service, ICurrentUser u, CancellationToken ct) => { var r = await service.CreateAsync(u.UserId, req, ct); return Results.Created($"api/notes/{r.Id}", r); });
        notesGroup.MapPut("{id:guid}", async (Guid id, UpdateNoteRequest req, NotesService service, ICurrentUser u, CancellationToken ct) => Results.Ok(await service.UpdateAsync(u.UserId, id, req, ct)));
        notesGroup.MapDelete("{id:guid}", async (Guid id, NotesService service, ICurrentUser u, CancellationToken ct) => { await service.ArchiveAsync(u.UserId, id, ct); return Results.NoContent(); });

        return builder;
    }
}
