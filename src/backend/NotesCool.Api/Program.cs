using NotesCool.Api.Extensions;
using NotesCool.Tasks.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddShared();
builder.Services.AddNotesModule(builder.Configuration);
builder.Services.AddTasksModule(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandling();
app.UseAuthentication();
app.UseAuthorization();

app.MapApiEndpoints();
app.MapTasksEndpoints();

app.Run();

public partial class Program { } // For integration tests
