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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotesCool.Shared.Auth;
using NotesCool.Tasks.Contracts;
using NotesCool.Tasks.Domain;
using NotesCool.Tasks.Infrastructure;
using Xunit;
using TaskStatus = NotesCool.Tasks.Domain.TaskStatus;

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
            builder.ConfigureTestServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TasksDbContext>));
                if (descriptor != null) services.Remove(descriptor);
                
                // Ensure a fresh DB name for this test class
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
        
        var createResponse = await client.PostAsJsonAsync("/api/tasks", new CreateTaskRequest("Test Task", "Desc", null));
        var task = await createResponse.Content.ReadFromJsonAsync<TaskDto>(JsonOptions);

        // Sending an invalid status enum (e.g. 99)
        var statusResponse = await client.PatchAsJsonAsync($"/api/tasks/{task!.Id}/status", new { status = 99 });

        statusResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LegacyTaskCompatibility_UndefinedStatusInDb_FallsBackToTodo()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
        
        var legacyTask = new TaskItem("Legacy", null, null, "kanban-user");
        
        // Use reflection to force an invalid status to simulate old DB row
        typeof(TaskItem).GetProperty("Status")!.SetValue(legacyTask, (TaskStatus)99);
        
        dbContext.Tasks.Add(legacyTask);
        await dbContext.SaveChangesAsync();

        var client = CreateClient("kanban-user");
        
        var getResponse = await client.GetAsync($"/api/tasks/{legacyTask.Id}");
        var task = await getResponse.Content.ReadFromJsonAsync<TaskDto>(JsonOptions);

        task.Should().NotBeNull();
        task!.Status.Should().Be(TaskStatus.Todo, "EF Core ValueConverter should fall back to Todo for undefined statuses.");
    }
}
