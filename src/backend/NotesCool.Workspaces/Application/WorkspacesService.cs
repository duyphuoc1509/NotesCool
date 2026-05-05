using Microsoft.EntityFrameworkCore;
using NotesCool.Shared.Errors;
using NotesCool.Workspaces.Contracts;
using NotesCool.Workspaces.Domain;
using NotesCool.Workspaces.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotesCool.Workspaces.Application;

public sealed class WorkspacesService(WorkspacesDbContext dbContext)
{
    public async Task<WorkspaceDto> CreateWorkspaceAsync(string ownerId, CreateWorkspaceRequest request)
    {
        var workspace = new Workspace(ownerId, request.Name, request.Description);
        dbContext.Workspaces.Add(workspace);
        await dbContext.SaveChangesAsync();

        return MapToDto(workspace);
    }

    public async Task<List<WorkspaceDto>> GetUserWorkspacesAsync(string userId)
    {
        var workspaces = await dbContext.Workspaces
            .Include(w => w.Members)
            .Where(w => w.Members.Any(m => m.UserId == userId))
            .ToListAsync();

        return workspaces.Select(MapToDto).ToList();
    }

    public async Task<WorkspaceDto> GetWorkspaceAsync(Guid id, string currentUserId)
    {
        var workspace = await GetWorkspaceWithAuthorizationAsync(id, currentUserId);
        return MapToDto(workspace);
    }

    public async Task<WorkspaceDto> UpdateWorkspaceAsync(Guid id, string currentUserId, UpdateWorkspaceRequest request)
    {
        var workspace = await GetWorkspaceWithAuthorizationAsync(id, currentUserId, requireAdminOrOwner: true);
        
        workspace.Update(request.Name, request.Description);
        await dbContext.SaveChangesAsync();

        return MapToDto(workspace);
    }

    public async Task ArchiveWorkspaceAsync(Guid id, string currentUserId)
    {
        var workspace = await dbContext.Workspaces
            .Include(w => w.Members)
            .FirstOrDefaultAsync(w => w.Id == id)
            ?? throw new ApiException("workspace_not_found", "Workspace not found.");

        var member = workspace.Members.FirstOrDefault(m => m.UserId == currentUserId)
            ?? throw new ApiException("access_denied", "Access denied.");

        if (member.Role != WorkspaceRole.Owner)
        {
            throw new ApiException("owner_required", "Only Owner can archive the workspace.");
        }

        workspace.Archive();
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<WorkspaceMemberDto>> GetMembersAsync(Guid workspaceId, string currentUserId)
    {
        var workspace = await GetWorkspaceWithAuthorizationAsync(workspaceId, currentUserId);
        return workspace.Members.Select(m => new WorkspaceMemberDto(m.UserId, m.Role, m.JoinedAtUtc)).ToList();
    }

    public async Task<WorkspaceMemberDto> AddMemberAsync(Guid workspaceId, string currentUserId, AddMemberRequest request)
    {
        var workspace = await GetWorkspaceWithAuthorizationAsync(workspaceId, currentUserId, requireAdminOrOwner: true);
        
        workspace.AddMember(request.UserId, request.Role);
        await dbContext.SaveChangesAsync();

        var newMember = workspace.Members.First(m => m.UserId == request.UserId);
        return new WorkspaceMemberDto(newMember.UserId, newMember.Role, newMember.JoinedAtUtc);
    }

    public async Task<WorkspaceMemberDto> UpdateMemberRoleAsync(Guid workspaceId, string memberUserId, string currentUserId, UpdateMemberRoleRequest request)
    {
        var workspace = await GetWorkspaceWithAuthorizationAsync(workspaceId, currentUserId, requireAdminOrOwner: true);
        var currentUserRole = workspace.Members.First(m => m.UserId == currentUserId).Role;
        
        workspace.UpdateMemberRole(memberUserId, request.Role, currentUserId, currentUserRole);
        await dbContext.SaveChangesAsync();

        var updatedMember = workspace.Members.First(m => m.UserId == memberUserId);
        return new WorkspaceMemberDto(updatedMember.UserId, updatedMember.Role, updatedMember.JoinedAtUtc);
    }

    public async Task RemoveMemberAsync(Guid workspaceId, string memberUserId, string currentUserId)
    {
        var workspace = await GetWorkspaceWithAuthorizationAsync(workspaceId, currentUserId, requireAdminOrOwner: true);
        var currentUserRole = workspace.Members.First(m => m.UserId == currentUserId).Role;
        
        workspace.RemoveMember(memberUserId, currentUserId, currentUserRole);
        await dbContext.SaveChangesAsync();
    }

    private async Task<Workspace> GetWorkspaceWithAuthorizationAsync(Guid id, string userId, bool requireAdminOrOwner = false)
    {
        var workspace = await dbContext.Workspaces
            .Include(w => w.Members)
            .FirstOrDefaultAsync(w => w.Id == id)
            ?? throw new ApiException("workspace_not_found", "Workspace not found.");

        var member = workspace.Members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new ApiException("access_denied", "Access denied.");

        if (requireAdminOrOwner && member.Role == WorkspaceRole.Member)
        {
            throw new ApiException("admin_or_owner_required", "Admin or Owner role is required.");
        }

        return workspace;
    }

    private static WorkspaceDto MapToDto(Workspace workspace) =>
        new(workspace.Id, workspace.Name, workspace.Description, workspace.OwnerId, workspace.CreatedAt, workspace.UpdatedAt);
}
