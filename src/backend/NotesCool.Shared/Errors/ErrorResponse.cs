namespace NotesCool.Shared.Errors;

public sealed record ErrorResponse(string Code, string Message, IDictionary<string, string[]>? Details = null);
