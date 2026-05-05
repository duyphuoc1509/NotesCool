using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NotesCool.Identity.Infrastructure;
using NotesCool.Workspaces.Infrastructure;
using NotesCool.Shared.Auth;
using NotesCool.Tasks.Contracts;
using NotesCool.Tasks.Domain.Entities;
using NotesCool.Tasks.Domain.Enums;
using NotesCool.Tasks.Infrastructure;
using WorkspaceEntity = NotesCool.Workspaces.Domain.Workspace;
using WorkspaceMemberEntity = NotesCool.Workspaces.Domain.WorkspaceMember;
using WorkspaceRoleEntity = NotesCool.Workspaces.Domain.WorkspaceRole;
using Xunit;

namespace NotesCool.Tasks.Tests.Api;

public class ProjectsEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public ProjectsEndpointsTests(WebApplicationFactory<Program> factory)
    {
        var dbName = $"InMemoryProjectsDb-{Guid.NewGuid()}";

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<TasksDbContext>>();
                services.RemoveAll<DbContextOptions<WorkspacesDbContext>>();
                services.RemoveAll<DbContextOptions<IdentityDbContext>>();

                services.AddDbContext<TasksDbContext>(options => options.UseInMemoryDatabase(dbName));
                services.AddDbContext<WorkspacesDbContext>(options => options.UseInMemoryDatabase($"{dbName}-workspaces"));
                services.AddDbContext<IdentityDbContext>(options => options.UseInMemoryDatabase($"{dbName}-identity"));
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
                services.AddScoped<ICurrentUser, TestCurrentUser>();
            });
        });

        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
    }

    [Fact]
    public async Task CreateProject_WhenWorkspaceMemberIsNotViewer_ReturnsCreated()
    {
        var workspace = await SeedWorkspaceAsync(WorkspaceRole.Member);

        var response = await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/projects", new CreateProjectRequest("Proj A", "Desc"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var payload = await response.Content.ReadFromJsonAsync<ProjectDto>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.Name.Should().Be("Proj A");
        payload.WorkspaceId.Should().Be(workspace.Id);
    }

    [Fact]
    public async Task CreateProject_WhenWorkspaceViewer_ReturnsBadRequestOrForbiddenExceptionSurface()
    {
        var workspace = await SeedWorkspaceAsync(WorkspaceRole.Viewer);

        var response = await _client.PostAsJsonAsync($"/api/workspaces/{workspace.Id}/projects", new CreateProjectRequest("Proj A", "Desc"));

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddProjectMember_WhenUserNotInWorkspace_ReturnsBadRequest()
    {
        var workspace = await SeedWorkspaceAsync(WorkspaceRole.Owner);
        var project = await SeedProjectAsync(workspace.Id, "test-user-id");

        var response = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/members", new AddProjectMemberRequest("outsider", ProjectRole.Member));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddProjectMember_WhenUserInWorkspace_ReturnsOk()
    {
        var workspace = await SeedWorkspaceAsync(WorkspaceRole.Owner);
        await SeedWorkspaceMemberAsync(workspace.Id, "user-2", WorkspaceRole.Member);
        var project = await SeedProjectAsync(workspace.Id, "test-user-id");

        var response = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/members", new AddProjectMemberRequest("user-2", ProjectRole.Member));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ProjectMemberDto>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.UserId.Should().Be("user-2");
        payload.Role.Should().Be(ProjectRole.Member);
    }

    [Fact]
    public async Task GetProjects_ByWorkspace_ReturnsWorkspaceProjects()
    {
        var workspace = await SeedWorkspaceAsync(WorkspaceRole.Member);
        await SeedProjectAsync(workspace.Id, "test-user-id", "Proj A");
        await SeedProjectAsync(workspace.Id, "test-user-id", "Proj B");

        var response = await _client.GetAsync($"/api/workspaces/{workspace.Id}/projects");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<List<ProjectDto>>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.Should().HaveCount(2);
    }

    private async Task<Workspace> SeedWorkspaceAsync(WorkspaceRole role)
    {
        using var scope = _factory.Services.CreateScope();
        var tasksDb = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
        var workspacesDb = scope.ServiceProvider.GetRequiredService<WorkspacesDbContext>();

        var workspace = new Workspace("Workspace A", "Desc", "test-user-id");
        tasksDb.Workspaces.Add(workspace);
        tasksDb.WorkspaceMembers.Add(new WorkspaceMember(workspace.Id, "test-user-id", role, "seed"));

        var workspaceEntityOwnerId = role == WorkspaceRole.Owner ? "test-user-id" : "seed-owner";
        var workspaceEntity = new WorkspaceEntity(workspaceEntityOwnerId, "Workspace A", "Desc");
        workspacesDb.Workspaces.Add(workspaceEntity);
        if (role != WorkspaceRole.Owner)
        {
            workspaceEntity.AddMember("test-user-id", ToWorkspaceRoleEntity(role));
        }
        workspacesDb.Entry(workspaceEntity).Property(x => x.Id).CurrentValue = workspace.Id;

        await tasksDb.SaveChangesAsync();
        await workspacesDb.SaveChangesAsync();
        return workspace;
    }

    private async Task SeedWorkspaceMemberAsync(Guid workspaceId, string userId, WorkspaceRole role)
    {
        using var scope = _factory.Services.CreateScope();
        var tasksDb = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
        var workspacesDb = scope.ServiceProvider.GetRequiredService<WorkspacesDbContext>();
        tasksDb.WorkspaceMembers.Add(new WorkspaceMember(workspaceId, userId, role, "seed"));

        var workspace = await workspacesDb.Workspaces.Include(x => x.Members).FirstAsync(x => x.Id == workspaceId);
        workspace.AddMember(userId, ToWorkspaceRoleEntity(role));

        await tasksDb.SaveChangesAsync();
        await workspacesDb.SaveChangesAsync();
    }

    private static WorkspaceRoleEntity ToWorkspaceRoleEntity(WorkspaceRole role) => role switch
    {
        WorkspaceRole.Owner => WorkspaceRoleEntity.Owner,
        WorkspaceRole.Admin => WorkspaceRoleEntity.Admin,
        WorkspaceRole.Viewer => WorkspaceRoleEntity.Viewer,
        _ => WorkspaceRoleEntity.Member
    };

    private async Task<Project> SeedProjectAsync(Guid workspaceId, string ownerId, string name = "Proj A")
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
        var project = new Project(workspaceId, name, "Desc", ownerId);
        db.Projects.Add(project);
        db.ProjectMembers.Add(new ProjectMember(project.Id, ownerId, ProjectRole.Manager, "seed"));
        await db.SaveChangesAsync();
        return project;
    }
}
