using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotesCool.Notes.Contracts;
using NotesCool.Notes.Infrastructure;
using NotesCool.Shared.Auth;
using Xunit;

namespace NotesCool.Notes.Tests.Api;

public class NotesOwnershipTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public NotesOwnershipTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureTestServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<NotesDbContext>));
                if (descriptor != null) services.Remove(descriptor);
                services.AddDbContext<NotesDbContext>(options => options.UseInMemoryDatabase("InMemoryNotesOwnershipDb"));
                services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions, DynamicTestAuthHandler>("Test", options => { });
                services.AddScoped<ICurrentUser, CurrentUser>();
            });
        });
    }

    [Fact]
    public async Task GetNote_WhenNotOwner_ReturnsNotFound()
    {
        // 1. User A creates a note
        var clientA = CreateClient("user-a");
        var createResponse = await clientA.PostAsJsonAsync("/api/notes", new CreateNoteRequest("Note A", "Content A"));
        var note = await createResponse.Content.ReadFromJsonAsync<NoteResponse>();

        // 2. User B tries to get User A's note
        var clientB = CreateClient("user-b");
        var getResponse = await clientB.GetAsync($"/api/notes/{note!.Id}");

        // 3. Should be NotFound (as per service implementation)
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateNote_WhenNotOwner_ReturnsNotFound()
    {
        var clientA = CreateClient("user-a");
        var createResponse = await clientA.PostAsJsonAsync("/api/notes", new CreateNoteRequest("Note A", "Content A"));
        var note = await createResponse.Content.ReadFromJsonAsync<NoteResponse>();

        var clientB = CreateClient("user-b");
        var updateResponse = await clientB.PutAsJsonAsync($"/api/notes/{note!.Id}", new UpdateNoteRequest("Note A Updated", "Content A Updated"));

        updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ArchiveNote_WhenNotOwner_ReturnsNotFound()
    {
        var clientA = CreateClient("user-a");
        var createResponse = await clientA.PostAsJsonAsync("/api/notes", new CreateNoteRequest("Note A", "Content A"));
        var note = await createResponse.Content.ReadFromJsonAsync<NoteResponse>();

        var clientB = CreateClient("user-b");
        var deleteResponse = await clientB.DeleteAsync($"/api/notes/{note!.Id}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private HttpClient CreateClient(string userId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", userId);
        return client;
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
