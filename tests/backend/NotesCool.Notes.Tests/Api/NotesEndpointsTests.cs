using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using NotesCool.Shared.Auth;
using NotesCool.Notes.Contracts;
using NotesCool.Notes.Infrastructure;
using Microsoft.AspNetCore.TestHost;

namespace NotesCool.Notes.Tests.Api;

public class NotesEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public NotesEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<NotesDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<NotesDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryNotesDb");
                });
                
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
                services.AddScoped<ICurrentUser, TestCurrentUser>();
            });
        }).CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
    }

    [Fact]
    public async Task GetNotes_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var unauthenticatedClient = _client;
        unauthenticatedClient.DefaultRequestHeaders.Authorization = null;

        var response = await unauthenticatedClient.GetAsync("/api/notes");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetNotes_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/notes");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task CreateNote_ReturnsOk()
    {
        var request = new CreateNoteRequest("Test Note", "Content");
        var response = await _client.PostAsJsonAsync("/api/notes", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var note = await response.Content.ReadFromJsonAsync<NoteResponse>();
        note.Should().NotBeNull();
        note!.Title.Should().Be("Test Note");
    }
}

public class TestCurrentUser : ICurrentUser
{
    public string UserId => "test-user-id";
    public string Role => SystemRoles.User;
    public bool IsInRole(string role) => role == Role;
}
