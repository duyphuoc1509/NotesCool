using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NotesCool.Shared.Auth;
using NotesCool.Tasks.Application;
using NotesCool.Tasks.Contracts;
using NotesCool.Tasks.Domain.Enums;

namespace NotesCool.Tasks.Contracts;

public static class ProjectEndpoints
{
    public static IEndpointRouteBuilder MapProjectsEndpoints(this IEndpointRouteBuilder builder)
    {
        // Projects under a workspace
        var workspaceGroup = builder.MapGroup("api/workspaces/{workspaceId:guid}/projects")
            .WithTags("Projects")
            .RequireAuthorization();

        workspaceGroup.MapGet("", async (
            [FromRoute] Guid workspaceId,
            [FromServices] ProjectsService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var results = await service.GetProjectsAsync(workspaceId, currentUser.UserId, ct);
            return Results.Ok(results);
        })
        .WithSummary("List projects in workspace")
        .Produces<List<ProjectDto>>();

        workspaceGroup.MapPost("", async (
            [FromRoute] Guid workspaceId,
            [FromBody] CreateProjectRequest request,
            [FromServices] ProjectsService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var result = await service.CreateProjectAsync(workspaceId, request, currentUser.UserId, ct);
            return Results.Created($"api/workspaces/{workspaceId}/projects/{result.Id}", result);
        })
        .WithSummary("Create project in workspace")
        .Produces<ProjectDto>(StatusCodes.Status201Created);

        // Single-project routes  
        var projectGroup = builder.MapGroup("api/projects/{projectId:guid}")
            .WithTags("Projects")
            .RequireAuthorization();

        projectGroup.MapGet("", async (
            [FromRoute] Guid projectId,
            [FromServices] ProjectsService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var result = await service.GetProjectAsync(projectId, currentUser.UserId, ct);
            return Results.Ok(result);
        })
        .WithSummary("Get project by ID")
        .Produces<ProjectDto>();

        projectGroup.MapPut("", async (
            [FromRoute] Guid projectId,
            [FromBody] UpdateProjectRequest request,
            [FromServices] ProjectsService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var result = await service.UpdateProjectAsync(projectId, request, currentUser.UserId, ct);
            return Results.Ok(result);
        })
        .WithSummary("Update project")
        .Produces<ProjectDto>();

        projectGroup.MapDelete("archive", async (
            [FromRoute] Guid projectId,
            [FromServices] ProjectsService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            await service.ArchiveProjectAsync(projectId, currentUser.UserId, ct);
            return Results.NoContent();
        })
        .WithSummary("Archive project")
        .Produces(StatusCodes.Status204NoContent);

        // Project members
        var memberGroup = builder.MapGroup("api/projects/{projectId:guid}/members")
            .WithTags("ProjectMembers")
            .RequireAuthorization();

        memberGroup.MapGet("", async (
            [FromRoute] Guid projectId,
            [FromServices] ProjectsService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var results = await service.GetMembersAsync(projectId, currentUser.UserId, ct);
            return Results.Ok(results);
        })
        .WithSummary("List project members")
        .Produces<List<ProjectMemberDto>>();

        memberGroup.MapPost("", async (
            [FromRoute] Guid projectId,
            [FromBody] AddProjectMemberRequest request,
            [FromServices] ProjectsService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var result = await service.AddMemberAsync(projectId, request, currentUser.UserId, ct);
            return Results.Ok(result);
        })
        .WithSummary("Add member to project (user must already be a workspace member)")
        .Produces<ProjectMemberDto>();

        memberGroup.MapPut("{userId}", async (
            [FromRoute] Guid projectId,
            [FromRoute] string userId,
            [FromBody] UpdateProjectMemberRoleRequest request,
            [FromServices] ProjectsService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var result = await service.UpdateMemberRoleAsync(projectId, userId, request.Role, currentUser.UserId, ct);
            return Results.Ok(result);
        })
        .WithSummary("Update project member role")
        .Produces<ProjectMemberDto>();

        memberGroup.MapDelete("{userId}", async (
            [FromRoute] Guid projectId,
            [FromRoute] string userId,
            [FromServices] ProjectsService service,
            [FromServices] ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            await service.RemoveMemberAsync(projectId, userId, currentUser.UserId, ct);
            return Results.NoContent();
        })
        .WithSummary("Remove member from project")
        .Produces(StatusCodes.Status204NoContent);

        return builder;
    }
}
