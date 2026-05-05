using NotesCool.Shared.Common;
using NotesCool.Shared.Errors;
using System.Collections.Generic;
using System.Linq;

namespace NotesCool.Workspaces.Domain;

public sealed class Workspace : Entity
{
    private readonly List<WorkspaceMember> _members = new();

    private Workspace() { }

    public Workspace(string ownerId, string name, string description)
    {
        if (string.IsNullOrWhiteSpace(ownerId)) throw new ApiException("owner_required", "Owner is required.");
        OwnerId = ownerId;
        Update(name, description);
        
        // Creator is the first Owner
        AddMember(ownerId, WorkspaceRole.Owner);
    }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public IReadOnlyCollection<WorkspaceMember> Members => _members.AsReadOnly();

    public void Update(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ApiException("workspace_name_required", "Workspace name is required.");
        if (name.Length > 200) throw new ApiException("workspace_name_too_long", "Workspace name must be 200 characters or fewer.");
        
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Touch();
    }

    public void AddMember(string userId, WorkspaceRole role)
    {
        if (_members.Any(m => m.UserId == userId))
        {
            throw new ApiException("member_already_exists", "User is already a member of this workspace.");
        }

        _members.Add(new WorkspaceMember(Id, userId, role));
        Touch();
    }

    public void UpdateMemberRole(string userId, WorkspaceRole newRole, string requestingUserId, WorkspaceRole requestingUserRole)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId) 
            ?? throw new ApiException("member_not_found", "Member not found.");

        if (userId == requestingUserId)
        {
            throw new ApiException("cannot_change_own_role", "Cannot change your own role.");
        }

        if (requestingUserRole == WorkspaceRole.Admin && member.Role == WorkspaceRole.Owner)
        {
            throw new ApiException("admin_cannot_modify_owner", "Admin cannot modify an Owner.");
        }

        if (member.Role == WorkspaceRole.Owner && newRole != WorkspaceRole.Owner)
        {
            var ownerCount = _members.Count(m => m.Role == WorkspaceRole.Owner);
            if (ownerCount <= 1)
            {
                throw new ApiException("cannot_remove_last_owner", "Cannot change role of the last Owner.");
            }
        }

        member.UpdateRole(newRole);
        Touch();
    }

    public void RemoveMember(string userId, string requestingUserId, WorkspaceRole requestingUserRole)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId) 
            ?? throw new ApiException("member_not_found", "Member not found.");

        if (requestingUserRole == WorkspaceRole.Admin && member.Role == WorkspaceRole.Owner)
        {
            throw new ApiException("admin_cannot_remove_owner", "Admin cannot remove an Owner.");
        }

        if (member.Role == WorkspaceRole.Owner)
        {
            var ownerCount = _members.Count(m => m.Role == WorkspaceRole.Owner);
            if (ownerCount <= 1)
            {
                throw new ApiException("cannot_remove_last_owner", "Cannot remove the last Owner.");
            }
        }

        _members.Remove(member);
        Touch();
    }
}

public enum WorkspaceRole
{
    Member,
    Admin,
    Owner
}
