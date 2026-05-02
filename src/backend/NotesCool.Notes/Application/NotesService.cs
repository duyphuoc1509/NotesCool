using Microsoft.EntityFrameworkCore;
using NotesCool.Notes.Contracts;
using NotesCool.Notes.Domain;
using NotesCool.Notes.Infrastructure;
using NotesCool.Shared.Common;
using NotesCool.Shared.Errors;

namespace NotesCool.Notes.Application;

public sealed class NotesService(NotesDbContext db)
{
    public async Task<PagedResult<NoteResponse>> SearchAsync(string ownerId, string? query, PageRequest page, CancellationToken ct)
    {
        var q = db.Notes.AsNoTracking().Where(n => n.OwnerId == ownerId);
        if (!string.IsNullOrWhiteSpace(query)) q = q.Where(n => n.Title.Contains(query) || n.Content.Contains(query));
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(n => n.UpdatedAt ?? n.CreatedAt).Skip(page.Skip).Take(page.Take).Select(n => ToResponse(n)).ToListAsync(ct);
        return new(items, page.Page, page.Take, total);
    }
    public async Task<NoteResponse> GetAsync(string ownerId, Guid id, CancellationToken ct) => ToResponse(await Owned(ownerId, id, ct));
    public async Task<NoteResponse> CreateAsync(string ownerId, CreateNoteRequest request, CancellationToken ct) { var note = new Note(ownerId, request.Title, request.Content); db.Notes.Add(note); await db.SaveChangesAsync(ct); return ToResponse(note); }
    public async Task<NoteResponse> UpdateAsync(string ownerId, Guid id, UpdateNoteRequest request, CancellationToken ct) { var note = await Owned(ownerId, id, ct); note.Update(request.Title, request.Content); await db.SaveChangesAsync(ct); return ToResponse(note); }
    public async Task<NoteResponse> SetFavoriteAsync(string ownerId, Guid id, bool isFavorite, CancellationToken ct) { var note = await Owned(ownerId, id, ct); note.SetFavorite(isFavorite); await db.SaveChangesAsync(ct); return ToResponse(note); }
    public async Task ArchiveAsync(string ownerId, Guid id, CancellationToken ct) { var note = await Owned(ownerId, id, ct); note.Archive(); await db.SaveChangesAsync(ct); }
    private async Task<Note> Owned(string ownerId, Guid id, CancellationToken ct) => await db.Notes.FirstOrDefaultAsync(n => n.Id == id && n.OwnerId == ownerId && n.ArchivedAt == null, ct) ?? throw new ApiException("note_not_found", "Note was not found.", 404);
    private static NoteResponse ToResponse(Note n) => new(n.Id, n.Title, n.Content, n.IsFavorite, n.CreatedAt, n.UpdatedAt, n.IsArchived);
}
