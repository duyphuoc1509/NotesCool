using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NotesCool.Shared.Auth;
using NotesCool.Workspaces.Application;
using NotesCool.Workspaces.Contracts;
using System;
using System.Threading.Tasks;

namespace NotesCool.Workspaces.Extensions;

public static class WorkspacesEndpointExtensions
{
    public static IEndpointRouteBuilder MapWorkspacesEndpoints(this IEndpointRouteBuilder builder)
    {
        var workspaceGroup = builder.MapGroup("api/workspaces")
            .WithTags("Workspaces")
            .RequireAuthorization();

        // Workspace CRUD
        workspaceGroup.MapGet("", async (ICurrentUser currentUser, WorkspacesService service) =>
        {
            var results = await service.GetUserWorkspacesAsync(currentUser.UserId);
            return Results.Ok(results);
        });

        workspaceGroup.MapPost("", async (CreateWorkspaceRequest request, ICurrentUser currentUser, WorkspacesService service) =>
        {
            var result = await service.CreateWorkspaceAsync(currentUser.UserId, request);
            return Results.Created($"/api/workspaces/{result.Id}", result);
        });

        workspaceGroup.MapGet("{id:guid}", async (Guid id, ICurrentUser currentUser, WorkspacesService service) =>
        {
            try
            {
                var result = await service.GetWorkspaceAsync(id, currentUser.UserId);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException) { return Results.NotFound(); }
        });

        workspaceGroup.MapPut("{id:guid}", async (Guid id, UpdateWorkspaceRequest request, ICurrentUser currentUser, WorkspacesService service) =>
        {
            var result = await service.UpdateWorkspaceAsync(id, currentUser.UserId, request);
            return Results.Ok(result);
        });

        workspaceGroup.MapDelete("{id:guid}", async (Guid id, ICurrentUser currentUser, WorkspacesService service) =>
        {
            await service.ArchiveWorkspaceAsync(id, currentUser.UserId);
            return Results.NoContent();
        });

        // Member APIs
        var memberGroup = workspaceGroup.MapGroup("{workspaceId:guid}/members");

        memberGroup.MapGet("", async (Guid workspaceId, ICurrentUser currentUser, WorkspacesService service) =>
        {
            var results = await service.GetMembersAsync(workspaceId, currentUser.UserId);
            return Results.Ok(results);
        });

        memberGroup.MapPost("", async (Guid workspaceId, AddMemberRequest request, ICurrentUser currentUser, WorkspacesService service) =>
        {
            var result = await service.AddMemberAsync(workspaceId, currentUser.UserId, request);
            return Results.Ok(result);
        });

        memberGroup.MapPut("{userId}", async (Guid workspaceId, string userId, UpdateMemberRoleRequest request, ICurrentUser currentUser, WorkspacesService service) =>
        {
            var result = await service.UpdateMemberRoleAsync(workspaceId, userId, currentUser.UserId, request);
            return Results.Ok(result);
        });

        memberGroup.MapDelete("{userId}", async (Guid workspaceId, string userId, ICurrentUser currentUser, WorkspacesService service) =>
        {
            await service.RemoveMemberAsync(workspaceId, userId, currentUser.UserId);
            return Results.NoContent();
        });

        return builder;
    }
}
