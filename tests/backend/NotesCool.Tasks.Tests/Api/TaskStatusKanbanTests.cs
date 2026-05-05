using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NotesCool.Identity.Infrastructure;
using NotesCool.Workspaces.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotesCool.Shared.Auth;
using NotesCool.Tasks.Contracts;
using NotesCool.Tasks.Domain.Entities;
using NotesCool.Tasks.Domain.Enums;
using NotesCool.Tasks.Infrastructure;
using Xunit;
using TaskStatus = NotesCool.Tasks.Domain.Enums.TaskStatus;

namespace NotesCool.Tasks.Tests.Api;

public class TaskStatusKanbanTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly WebApplicationFactory<Program> _factory;

    public TaskStatusKanbanTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureTestServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TasksDbContext>));
                if (descriptor != null) services.Remove(descriptor);
                
                var dbName = "InMemoryTaskStatusDb_" + Guid.NewGuid().ToString();
                services.AddDbContext<TasksDbContext>(options => options.UseInMemoryDatabase(dbName));
                services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions, DynamicTestAuthHandler>("Test", options => { });
                services.AddScoped<ICurrentUser, CurrentUser>();
            });
        });
    }

    private HttpClient CreateClient(string userId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", userId);
        return client;
    }

    [Fact]
    public async Task ChangeStatus_ToInvalidValue_ReturnsBadRequest()
    {
        var client = CreateClient("kanban-user");
        var project = await SeedProjectGraphAsync("kanban-user");
        
        var createResponse = await client.PostAsJsonAsync($"/api/projects/{project.Id}/tasks", new CreateTaskRequest("Test Task", "Desc"));
        var task = await createResponse.Content.ReadFromJsonAsync<TaskDto>(JsonOptions);

        var statusResponse = await client.PatchAsJsonAsync($"/api/tasks/{task!.Id}/status", new { status = 99 });

        statusResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LegacyTaskCompatibility_UndefinedStatusInDb_FallsBackToTodo()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
        
        var legacyTask = new TaskItem(Guid.Empty, Guid.Empty, "Legacy", null, TaskPriority.Medium, null, "kanban-user");
        
        typeof(TaskItem).GetProperty("Status")!.SetValue(legacyTask, (TaskStatus)99);
        
        dbContext.Tasks.Add(legacyTask);
        await dbContext.SaveChangesAsync();

        var client = CreateClient("kanban-user");
        
        // This fails if TaskOwnership check kicks in since Guid.Empty project doesn't exist/no member.
        // But for this compat test, we can seed a member for Guid.Empty project or just check domain property.
        // Better: test domain property via UnitTest if needed, but here we test API.
        // Let's seed project/member for it.
        var workspace = new Workspace("WS", null, "kanban-user");
        typeof(Workspace).GetProperty("Id")!.SetValue(workspace, Guid.Empty);
        var project = new Project(Guid.Empty, "PR", null, "kanban-user");
        typeof(Project).GetProperty("Id")!.SetValue(project, Guid.Empty);
        
        dbContext.Workspaces.Add(workspace);
        dbContext.Projects.Add(project);
        dbContext.WorkspaceMembers.Add(new WorkspaceMember(Guid.Empty, "kanban-user", WorkspaceRole.Member, "seed"));
        dbContext.ProjectMembers.Add(new ProjectMember(Guid.Empty, "kanban-user", ProjectRole.Member, "seed"));
        await dbContext.SaveChangesAsync();

        var getResponse = await client.GetAsync($"/api/tasks/{legacyTask.Id}");
        var task = await getResponse.Content.ReadFromJsonAsync<TaskDto>(JsonOptions);

        task.Should().NotBeNull();
        task!.Status.Should().Be(TaskStatus.Todo);
    }

    private async Task<Project> SeedProjectGraphAsync(string userId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TasksDbContext>();

        var workspace = new Workspace("Workspace", null, userId);
        db.Workspaces.Add(workspace);
        db.WorkspaceMembers.Add(new WorkspaceMember(workspace.Id, userId, WorkspaceRole.Member, "seed"));

        var project = new Project(workspace.Id, "Project", null, userId);
        db.Projects.Add(project);
        db.ProjectMembers.Add(new ProjectMember(project.Id, userId, ProjectRole.Manager, "seed"));
        await db.SaveChangesAsync();
        return project;
    }
}
