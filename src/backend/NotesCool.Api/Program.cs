using NotesCool.Api.Auth;
using NotesCool.Api.Extensions;
using NotesCool.Tasks.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddShared(builder.Configuration);
builder.Services.AddNotesModule(builder.Configuration);
builder.Services.AddTasksModule(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NotesCool API v1");
    c.RoutePrefix = "swagger";
});

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapApiEndpoints();
app.MapTasksEndpoints();

app.Run();

public partial class Program { } // For integration tests
