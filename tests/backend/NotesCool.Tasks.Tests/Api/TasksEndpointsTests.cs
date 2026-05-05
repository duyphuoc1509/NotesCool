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
using NotesCool.Shared.Auth;
using NotesCool.Shared.Common;
using NotesCool.Tasks.Contracts;
using NotesCool.Tasks.Domain.Entities;
using NotesCool.Tasks.Domain.Enums;
using NotesCool.Tasks.Infrastructure;
using Xunit;
using TaskStatus = NotesCool.Tasks.Domain.Enums.TaskStatus;

namespace NotesCool.Tasks.Tests.Api;

public class TasksEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public TasksEndpointsTests(WebApplicationFactory<Program> factory)
    {
        var dbName = $"InMemoryTasksDb-{Guid.NewGuid()}";
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TasksDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<TasksDbContext>(options => options.UseInMemoryDatabase(dbName));
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
                services.AddScoped<ICurrentUser, TestCurrentUser>();
            });
        });

        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
    }

    [Fact]
    public async Task GetTasks_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var unauthenticatedClient = _factory.CreateClient();
        var workspace = await SeedWorkspaceAsync(WorkspaceRole.Member);
        var project = await SeedProjectAsync(workspace.Id, "test-user-id");

        var response = await unauthenticatedClient.GetAsync($"/api/projects/{project.Id}/tasks");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTask_ReturnsCreated()
    {
        var workspace = await SeedWorkspaceAsync(WorkspaceRole.Member);
        var project = await SeedProjectAsync(workspace.Id, "test-user-id");

        var request = new CreateTaskRequest("Test Task", "Desc", TaskPriority.High, null, ["test-user-id"]);
        var response = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/tasks", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var task = await response.Content.ReadFromJsonAsync<TaskDto>(JsonOptions);
        task.Should().NotBeNull();
        task!.Title.Should().Be("Test Task");
        task.Priority.Should().Be(TaskPriority.High);
        task.Assignees.Should().ContainSingle(a => a.UserId == "test-user-id");
    }

    [Fact]
    public async Task GetTasks_FilterByAssignee_ReturnsMatchingTasks()
    {
        var workspace = await SeedWorkspaceAsync(WorkspaceRole.Member);
        await SeedWorkspaceMemberAsync(workspace.Id, "user-2", WorkspaceRole.Member);
        var project = await SeedProjectAsync(workspace.Id, "test-user-id");
        await SeedProjectMemberAsync(project.Id, "user-2", ProjectRole.Member);

        var task1 = await CreateTaskAsync(project.Id, "Task 1", assignees: ["user-2"]);
        await CreateTaskAsync(project.Id, "Task 2");

        var response = await _client.GetAsync($"/api/projects/{project.Id}/tasks?assigneeId=user-2");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<PagedResult<TaskDto>>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.Items.Should().ContainSingle(t => t.Id == task1.Id);
    }

    [Fact]
    public async Task CreateSubTask_ForSubTask_ReturnsBadRequest()
    {
        var workspace = await SeedWorkspaceAsync(WorkspaceRole.Member);
        var project = await SeedProjectAsync(workspace.Id, "test-user-id");
        var parent = await CreateTaskAsync(project.Id, "Parent");

        var subtaskResponse = await _client.PostAsJsonAsync($"/api/tasks/{parent.Id}/subtasks", new CreateTaskRequest("Sub 1", null));
        subtaskResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var subtask = await subtaskResponse.Content.ReadFromJsonAsync<TaskDto>(JsonOptions);

        var invalidResponse = await _client.PostAsJsonAsync($"/api/tasks/{subtask!.Id}/subtasks", new CreateTaskRequest("SubSub", null));
        invalidResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AssignUser_OutsideProject_ReturnsBadRequest()
    {
        var workspace = await SeedWorkspaceAsync(WorkspaceRole.Member);
        await SeedWorkspaceMemberAsync(workspace.Id, "user-2", WorkspaceRole.Member);
        var project = await SeedProjectAsync(workspace.Id, "test-user-id");
        var task = await CreateTaskAsync(project.Id, "Task 1");

        var response = await _client.PostAsJsonAsync($"/api/tasks/{task.Id}/assignees", new AddTaskAssigneeRequest("user-2"));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetActivityLogs_AfterCreateAndAssign_ReturnsEntries()
    {
        var workspace = await SeedWorkspaceAsync(WorkspaceRole.Member);
        var project = await SeedProjectAsync(workspace.Id, "test-user-id");
        var task = await CreateTaskAsync(project.Id, "Task 1");

        var assignResponse = await _client.PostAsJsonAsync($"/api/tasks/{task.Id}/assignees", new AddTaskAssigneeRequest("test-user-id"));
        assignResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var logsResponse = await _client.GetAsync($"/api/tasks/{task.Id}/activity-logs");
        logsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var logs = await logsResponse.Content.ReadFromJsonAsync<List<TaskActivityLogDto>>(JsonOptions);
        logs.Should().NotBeNull();
        logs!.Should().Contain(l => l.Action == "Created");
        logs.Should().Contain(l => l.Action == "AssignedUser");
    }

    private async Task<Workspace> SeedWorkspaceAsync(WorkspaceRole role)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TasksDbContext>();

        var workspace = new Workspace("Workspace A", "Desc", "test-user-id");
        db.Workspaces.Add(workspace);
        db.WorkspaceMembers.Add(new WorkspaceMember(workspace.Id, "test-user-id", role, "seed"));
        await db.SaveChangesAsync();
        return workspace;
    }

    private async Task SeedWorkspaceMemberAsync(Guid workspaceId, string userId, WorkspaceRole role)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
        db.WorkspaceMembers.Add(new WorkspaceMember(workspaceId, userId, role, "seed"));
        await db.SaveChangesAsync();
    }

    private async Task<Project> SeedProjectAsync(Guid workspaceId, string ownerId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
        var project = new Project(workspaceId, "Proj A", "Desc", ownerId);
        db.Projects.Add(project);
        db.ProjectMembers.Add(new ProjectMember(project.Id, ownerId, ProjectRole.Manager, "seed"));
        await db.SaveChangesAsync();
        return project;
    }

    private async Task SeedProjectMemberAsync(Guid projectId, string userId, ProjectRole role)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
        db.ProjectMembers.Add(new ProjectMember(projectId, userId, role, "seed"));
        await db.SaveChangesAsync();
    }

    private async Task<TaskDto> CreateTaskAsync(Guid projectId, string title, IReadOnlyList<string>? assignees = null)
    {
        var response = await _client.PostAsJsonAsync($"/api/projects/{projectId}/tasks", new CreateTaskRequest(title, "Desc", TaskPriority.Medium, null, assignees));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<TaskDto>(JsonOptions))!;
    }
}

public class TestCurrentUser : ICurrentUser
{
    public string UserId => "test-user-id";
    public string Role => "User";
    public bool IsInRole(string role) => Role == role;
}
