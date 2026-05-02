using NotesCool.Shared.Common;
using NotesCool.Shared.Errors;

namespace NotesCool.Notes.Domain;

public sealed class Note : Entity
{
    private Note() { }
    public Note(string ownerId, string title, string content)
    {
        OwnerId = string.IsNullOrWhiteSpace(ownerId) ? throw new ApiException("owner_required", "Owner is required.") : ownerId;
        Update(title, content);
    }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public bool IsFavorite { get; private set; }
    public void Update(string title, string content)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ApiException("note_title_required", "Note title is required.");
        if (title.Length > 200) throw new ApiException("note_title_too_long", "Note title must be 200 characters or fewer.");
        Title = title.Trim();
        Content = content?.Trim() ?? string.Empty;
        Touch();
    }

    public void SetFavorite(bool isFavorite)
    {
        if (IsFavorite == isFavorite) return;

        IsFavorite = isFavorite;
        Touch();
    }
}
