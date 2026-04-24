namespace NotesCool.Notes.Contracts;

public sealed record CreateNoteRequest(string Title, string Content);
public sealed record UpdateNoteRequest(string Title, string Content);
public sealed record NoteResponse(Guid Id, string Title, string Content, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
