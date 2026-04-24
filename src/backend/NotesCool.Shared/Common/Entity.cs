namespace NotesCool.Shared.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public string OwnerId { get; protected set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; protected set; }
    public DateTimeOffset? ArchivedAt { get; protected set; }
    public bool IsArchived => ArchivedAt.HasValue;
    public void Archive() => ArchivedAt ??= DateTimeOffset.UtcNow;
    protected void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
