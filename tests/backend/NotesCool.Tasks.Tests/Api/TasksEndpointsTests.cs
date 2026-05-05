using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
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
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TasksDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<TasksDbContext>(options => options.UseInMemoryDatabase("InMemoryTasksDb"));
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
        var response = await unauthenticatedClient.GetAsync("/api/tasks");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTasks_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/tasks");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateTask_ReturnsOk()
    {
        var request = new CreateTaskRequest("Test Task", "Desc", null);
        var response = await _client.PostAsJsonAsync("/api/tasks", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var task = await response.Content.ReadFromJsonAsync<TaskDto>(JsonOptions);
        task.Should().NotBeNull();
        task!.Title.Should().Be("Test Task");
        task.IsFavorite.Should().BeFalse();
        task.Status.Should().Be(TaskStatus.Todo);
    }

    [Fact]
    public async Task ChangeTaskStatus_WithStringEnumPayload_ReturnsUpdatedStatus()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", new CreateTaskRequest("Kanban Task", "Desc", null));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskDto>(JsonOptions);
        createdTask.Should().NotBeNull();

        var statusResponse = await _client.PatchAsJsonAsync($"/api/tasks/{createdTask!.Id}/status", new { status = "InReview" });
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedTask = await statusResponse.Content.ReadFromJsonAsync<TaskDto>(JsonOptions);
        updatedTask.Should().NotBeNull();
        updatedTask!.Status.Should().Be(TaskStatus.InReview);
    }

    [Fact]
    public async Task SetFavorite_ReturnsUpdatedTask()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", new CreateTaskRequest("Fav Task", "Desc", null));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskDto>(JsonOptions);
        createdTask.Should().NotBeNull();

        var favoriteResponse = await _client.PatchAsJsonAsync($"/api/tasks/{createdTask!.Id}/favorite", new { isFavorite = true });
        favoriteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedTask = await favoriteResponse.Content.ReadFromJsonAsync<TaskDto>(JsonOptions);
        updatedTask.Should().NotBeNull();
        updatedTask!.IsFavorite.Should().BeTrue();
    }

    [Fact]
    public async Task GetTasks_WithArchivedStatus_ReturnsArchivedTasks()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TasksDbContext>();

        var archivedTask = new TaskItem(Guid.Empty, Guid.Empty, "Archived Task",  "Desc", TaskPriority.Medium,  null,  "test-user-id");
        archivedTask.ChangeStatus(TaskStatus.Archived, "tester");
        archivedTask.Archive();

        dbContext.Tasks.Add(archivedTask);
        await dbContext.SaveChangesAsync();

        var response = await _client.GetAsync("/api/tasks?status=Archived");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<PagedResult<TaskDto>>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.Items.Should().ContainSingle(t => t.Id == archivedTask.Id && t.Status == TaskStatus.Archived);
    }
}

public class TestCurrentUser : ICurrentUser
{
    public string UserId => "test-user-id";
    public string Role => "User";
    public bool IsInRole(string role) => Role == role;
}
