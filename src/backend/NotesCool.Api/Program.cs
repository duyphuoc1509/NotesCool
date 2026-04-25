using NotesCool.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddShared();
builder.Services.AddNotesModule(builder.Configuration);
builder.Services.AddTasksModule(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();

app.MapNotesEndpoints();
app.MapTasksEndpoints();

app.Run();

public partial class Program { } // For integration tests
