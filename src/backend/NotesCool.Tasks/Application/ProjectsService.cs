using Microsoft.EntityFrameworkCore;
using NotesCool.Shared.Errors;
using NotesCool.Tasks.Contracts;
using NotesCool.Tasks.Domain.Entities;
using NotesCool.Tasks.Domain.Enums;
using NotesCool.Tasks.Infrastructure;

namespace NotesCool.Tasks.Application;

public sealed class ProjectsService(TasksDbContext db)
{
    public async Task<List<ProjectDto>> GetProjectsAsync(Guid workspaceId, string userId, CancellationToken ct)
    {
        // Must be a member of the workspace to see its projects
        await EnsureWorkspaceMemberAsync(workspaceId, userId, ct);

        var projects = await db.Projects
            .AsNoTracking()
            .Where(p => p.WorkspaceId == workspaceId && p.ArchivedAt == null)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => MapToDto(p))
            .ToListAsync(ct);

        return projects;
    }

    public async Task<ProjectDto> GetProjectAsync(Guid id, string userId, CancellationToken ct)
    {
        var project = await db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new ApiException("project_not_found", "Project not found.");

        await EnsureProjectMemberAsync(id, userId, ct);

        return MapToDto(project);
    }

    public async Task<ProjectDto> CreateProjectAsync(Guid workspaceId, CreateProjectRequest request, string userId, CancellationToken ct)
    {
        // BA rule: Workspace Owner/Admin/Member can create project; Viewer cannot.
        var workspaceMember = await db.WorkspaceMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId && m.IsActive, ct)
            ?? throw new ApiException("access_denied", "You are not a member of this workspace.");

        if (workspaceMember.Role == WorkspaceRole.Viewer)
        {
            throw new ApiException("access_denied", "Workspace Viewer cannot create projects.");
        }

        var project = new Project(workspaceId, request.Name, request.Description, userId);
        db.Projects.Add(project);

        // Auto-add creator as Manager
        var member = new ProjectMember(project.Id, userId, ProjectRole.Manager, userId);
        db.ProjectMembers.Add(member);

        await db.SaveChangesAsync(ct);

        return MapToDto(project);
    }

    public async Task<ProjectDto> UpdateProjectAsync(Guid id, UpdateProjectRequest request, string userId, CancellationToken ct)
    {
        var project = await db.Projects
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new ApiException("project_not_found", "Project not found.");

        // Must be Project Manager to update
        var member = await db.ProjectMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ProjectId == id && m.UserId == userId, ct)
            ?? throw new ApiException("access_denied", "Access denied.");

        if (member.Role != ProjectRole.Manager)
        {
            throw new ApiException("access_denied", "Only Project Managers can update project details.");
        }

        project.Update(request.Name, request.Description);
        await db.SaveChangesAsync(ct);

        return MapToDto(project);
    }

    public async Task ArchiveProjectAsync(Guid id, string userId, CancellationToken ct)
    {
        var project = await db.Projects
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new ApiException("project_not_found", "Project not found.");

        // Must be Project Manager to archive
        var member = await db.ProjectMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ProjectId == id && m.UserId == userId, ct)
            ?? throw new ApiException("access_denied", "Access denied.");

        if (member.Role != ProjectRole.Manager)
        {
            throw new ApiException("access_denied", "Only Project Managers can archive projects.");
        }

        project.Archive();
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<ProjectMemberDto>> GetMembersAsync(Guid projectId, string userId, CancellationToken ct)
    {
        await EnsureProjectMemberAsync(projectId, userId, ct);

        return await db.ProjectMembers
            .AsNoTracking()
            .Where(m => m.ProjectId == projectId && m.IsActive)
            .Select(m => MapMemberToDto(m))
            .ToListAsync(ct);
    }

    public async Task<ProjectMemberDto> AddMemberAsync(Guid projectId, AddProjectMemberRequest request, string userId, CancellationToken ct)
    {
        var project = await db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId, ct)
            ?? throw new ApiException("project_not_found", "Project not found.");

        // Authorization check: Must be Project Manager
        var currentMember = await db.ProjectMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId, ct)
            ?? throw new ApiException("access_denied", "Access denied.");

        if (currentMember.Role != ProjectRole.Manager)
        {
            throw new ApiException("access_denied", "Only Project Managers can add members.");
        }

        // Rule: Project member must already belong to workspace
        var workspaceMember = await db.WorkspaceMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.WorkspaceId == project.WorkspaceId && m.UserId == request.UserId, ct);

        if (workspaceMember == null || !workspaceMember.IsActive)
        {
            throw new ApiException("user_not_in_workspace", "User must be a member of the workspace before being added to a project.");
        }

        // Check if already a member
        var existingMember = await db.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == request.UserId, ct);

        if (existingMember != null)
        {
            if (existingMember.IsActive)
            {
                throw new ApiException("already_member", "User is already a member of this project.");
            }

            existingMember.Reactivate(request.Role);
        }
        else
        {
            var newMember = new ProjectMember(projectId, request.UserId, request.Role, userId);
            db.ProjectMembers.Add(newMember);
        }

        await db.SaveChangesAsync(ct);

        var memberResult = await db.ProjectMembers
            .AsNoTracking()
            .FirstAsync(m => m.ProjectId == projectId && m.UserId == request.UserId, ct);

        return MapMemberToDto(memberResult);
    }

    public async Task<ProjectMemberDto> UpdateMemberRoleAsync(Guid projectId, string memberUserId, ProjectRole role, string userId, CancellationToken ct)
    {
        await EnsureProjectManagerAsync(projectId, userId, ct);

        var member = await db.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == memberUserId, ct)
            ?? throw new ApiException("member_not_found", "Project member not found.");

        member.UpdateRole(role);
        await db.SaveChangesAsync(ct);

        return MapMemberToDto(member);
    }

    public async Task RemoveMemberAsync(Guid projectId, string memberUserId, string userId, CancellationToken ct)
    {
        await EnsureProjectManagerAsync(projectId, userId, ct);

        var member = await db.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == memberUserId, ct)
            ?? throw new ApiException("member_not_found", "Project member not found.");

        // Rule: cannot remove last manager? (Not explicitly in AC but good practice)
        
        member.Deactivate();
        await db.SaveChangesAsync(ct);
    }

    private async Task EnsureWorkspaceMemberAsync(Guid workspaceId, string userId, CancellationToken ct)
    {
        var exists = await db.WorkspaceMembers
            .AsNoTracking()
            .AnyAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId && m.IsActive, ct);

        if (!exists) throw new ApiException("access_denied", "You are not a member of this workspace.");
    }

    private async Task EnsureProjectMemberAsync(Guid projectId, string userId, CancellationToken ct)
    {
        var exists = await db.ProjectMembers
            .AsNoTracking()
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId && m.IsActive, ct);

        if (!exists) throw new ApiException("access_denied", "You are not a member of this project.");
    }

    private async Task EnsureProjectManagerAsync(Guid projectId, string userId, CancellationToken ct)
    {
        var member = await db.ProjectMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId && m.IsActive, ct)
            ?? throw new ApiException("access_denied", "You are not a member of this project.");

        if (member.Role != ProjectRole.Manager)
        {
            throw new ApiException("access_denied", "Project Manager role is required.");
        }
    }

    private static ProjectDto MapToDto(Project p) =>
        new(p.Id, p.WorkspaceId, p.Name, p.Description, p.OwnerId, p.CreatedAt, p.UpdatedAt, p.IsArchived);

    private static ProjectMemberDto MapMemberToDto(ProjectMember m) =>
        new(m.ProjectId, m.UserId, m.Role, m.AddedBy, m.IsActive, m.CreatedAt, m.UpdatedAt);
}
