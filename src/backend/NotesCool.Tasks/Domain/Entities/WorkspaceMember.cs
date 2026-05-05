using NotesCool.Shared.Common;
using NotesCool.Tasks.Domain.Enums;

namespace NotesCool.Tasks.Domain.Entities;

public class WorkspaceMember : Entity
{
    public Guid WorkspaceId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public WorkspaceRole Role { get; private set; }
    public string? InvitedBy { get; private set; }
    public bool IsActive { get; private set; }

    protected WorkspaceMember() { }

    public WorkspaceMember(Guid workspaceId, string userId, WorkspaceRole role, string? invitedBy)
    {
        WorkspaceId = workspaceId;
        UserId = userId;
        Role = role;
        InvitedBy = invitedBy;
        IsActive = true;
    }

    public void UpdateRole(WorkspaceRole role)
    {
        Role = role;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }
}
