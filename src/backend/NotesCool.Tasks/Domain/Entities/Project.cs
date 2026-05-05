using System.ComponentModel.DataAnnotations;
using NotesCool.Shared.Common;

namespace NotesCool.Tasks.Domain.Entities;

public class Project : Entity
{
    public Guid WorkspaceId { get; private set; }
    
    [Required]
    [MaxLength(150)]
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    protected Project() { }

    public Project(Guid workspaceId, string name, string? description, string ownerId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required", nameof(name));
        WorkspaceId = workspaceId;
        Name = name;
        Description = description;
        OwnerId = ownerId;
    }

    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required", nameof(name));
        Name = name;
        Description = description;
        Touch();
    }
}
