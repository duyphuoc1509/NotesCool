using NotesCool.Api.Contracts;
using NotesCool.Api.Extensions;
using NotesCool.Api.Identity;
using NotesCool.Tasks.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddShared(builder.Configuration);
builder.Services.AddShared(builder.Configuration, builder.Environment);
builder.Services.AddNotesModule(builder.Configuration);
builder.Services.AddTasksModule(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapApiEndpoints();
app.MapTasksEndpoints();
app.MapAuthEndpoints();
app.MapSsoEndpoints();

app.Run();

app.Run();

public partial class Program { } // For integration tests