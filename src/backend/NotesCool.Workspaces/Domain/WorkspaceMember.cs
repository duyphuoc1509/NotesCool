using System;

namespace NotesCool.Workspaces.Domain;

public sealed class WorkspaceMember
{
    private WorkspaceMember() { }

    internal WorkspaceMember(string workspaceId, string userId, WorkspaceRole role)
    {
        WorkspaceId = workspaceId;
        UserId = userId;
        Role = role;
        JoinedAtUtc = DateTime.UtcNow;
    }

    public string WorkspaceId { get; private set; } = string.Empty;
    public string UserId { get; private set; } = string.Empty;
    public WorkspaceRole Role { get; private set; }
    public DateTime JoinedAtUtc { get; private set; }

    internal void UpdateRole(WorkspaceRole role)
    {
        Role = role;
    }
}
