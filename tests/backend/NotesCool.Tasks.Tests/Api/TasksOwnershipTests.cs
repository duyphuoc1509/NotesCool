using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
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

public class TasksOwnershipTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TasksOwnershipTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureTestServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TasksDbContext>));
                if (descriptor != null) services.Remove(descriptor);
                services.AddDbContext<TasksDbContext>(options => options.UseInMemoryDatabase("InMemoryTasksOwnershipDb" + Guid.NewGuid()));
                services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions, DynamicTestAuthHandler>("Test", options => { });
                services.AddScoped<ICurrentUser, CurrentUser>();
            });
        });
    }

    [Fact]
    public async Task GetTask_WhenNotProjectMember_ReturnsBadRequest()
    {
        var project = await SeedProjectGraphAsync();

        var clientA = CreateClient("user-a");
        var createResponse = await clientA.PostAsJsonAsync($"/api/projects/{project.Id}/tasks", new CreateTaskRequest("Task A", "Desc A", TaskPriority.Medium, null));
        var task = await createResponse.Content.ReadFromJsonAsync<TaskDto>();

        var clientB = CreateClient("user-b");
        var getResponse = await clientB.GetAsync($"/api/tasks/{task!.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTask_WhenNotProjectMember_ReturnsBadRequest()
    {
        var project = await SeedProjectGraphAsync();

        var clientA = CreateClient("user-a");
        var createResponse = await clientA.PostAsJsonAsync($"/api/projects/{project.Id}/tasks", new CreateTaskRequest("Task A", "Desc A", TaskPriority.Medium, null));
        var task = await createResponse.Content.ReadFromJsonAsync<TaskDto>();

        var clientB = CreateClient("user-b");
        var updateResponse = await clientB.PutAsJsonAsync($"/api/tasks/{task!.Id}", new UpdateTaskRequest("Task A Updated", "Desc A Updated", TaskPriority.High, null));

        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangeStatus_WhenNotProjectMember_ReturnsBadRequest()
    {
        var project = await SeedProjectGraphAsync();

        var clientA = CreateClient("user-a");
        var createResponse = await clientA.PostAsJsonAsync($"/api/projects/{project.Id}/tasks", new CreateTaskRequest("Task A", "Desc A", TaskPriority.Medium, null));
        var task = await createResponse.Content.ReadFromJsonAsync<TaskDto>();

        var clientB = CreateClient("user-b");
        var statusResponse = await clientB.PatchAsJsonAsync($"/api/tasks/{task!.Id}/status", new ChangeTaskStatusRequest(TaskStatus.InProgress));

        statusResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteTask_WhenNotProjectMember_ReturnsBadRequest()
    {
        var project = await SeedProjectGraphAsync();

        var clientA = CreateClient("user-a");
        var createResponse = await clientA.PostAsJsonAsync($"/api/projects/{project.Id}/tasks", new CreateTaskRequest("Task A", "Desc A", TaskPriority.Medium, null));
        var task = await createResponse.Content.ReadFromJsonAsync<TaskDto>();

        var clientB = CreateClient("user-b");
        var deleteResponse = await clientB.DeleteAsync($"/api/tasks/{task!.Id}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private HttpClient CreateClient(string userId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", userId);
        return client;
    }

    private async Task<Project> SeedProjectGraphAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TasksDbContext>();

        var workspace = new Workspace("Workspace", null, "user-a");
        db.Workspaces.Add(workspace);
        db.WorkspaceMembers.Add(new WorkspaceMember(workspace.Id, "user-a", WorkspaceRole.Member, "seed"));

        var project = new Project(workspace.Id, "Project", null, "user-a");
        db.Projects.Add(project);
        db.ProjectMembers.Add(new ProjectMember(project.Id, "user-a", ProjectRole.Manager, "seed"));
        await db.SaveChangesAsync();
        return project;
    }
}

public class DynamicTestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public DynamicTestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, System.Text.Encodings.Web.UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Request.Headers.Authorization.ToString().Replace("Test ", "");
        if (string.IsNullOrEmpty(userId)) userId = "test-user";

        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, $"{userId}@example.com"),
            new Claim(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
