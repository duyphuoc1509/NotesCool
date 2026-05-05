using NotesCool.Shared.Common;
using NotesCool.Tasks.Domain.Enums;

namespace NotesCool.Tasks.Domain.Entities;

public class ProjectMember : Entity
{
    public Guid ProjectId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public ProjectRole Role { get; private set; }
    public string? AddedBy { get; private set; }
    public bool IsActive { get; private set; }

    protected ProjectMember() { }

    public ProjectMember(Guid projectId, string userId, ProjectRole role, string? addedBy)
    {
        ProjectId = projectId;
        UserId = userId;
        Role = role;
        AddedBy = addedBy;
        IsActive = true;
    }

    public void UpdateRole(ProjectRole role)
    {
        Role = role;
        Touch();
    }

    public void Reactivate(ProjectRole role)
    {
        Role = role;
        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }
}
