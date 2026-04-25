using NotesCool.Api.Extensions;
using NotesCool.Tasks.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddShared();
builder.Services.AddNotesModule(builder.Configuration);
builder.Services.AddTasksModule(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();

app.MapApiEndpoints();
app.MapTasksEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NotesCool API V1");
    });
}

app.Run();

public partial class Program { } // For integration tests
