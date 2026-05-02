using System.ComponentModel.DataAnnotations;

namespace NotesCool.Notes.Contracts;

public sealed record CreateNoteRequest(
    [Required]
    [StringLength(200)]
    string Title, 
    [Required]
    string Content);

public sealed record UpdateNoteRequest(
    [Required]
    [StringLength(200)]
    string Title, 
    [Required]
    string Content);

public sealed record NoteResponse(Guid Id, string Title, string Content, bool IsFavorite, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt, bool IsArchived);
