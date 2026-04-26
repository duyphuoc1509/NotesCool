using NotesCool.Api.Contracts;
using NotesCool.Api.Extensions;
using NotesCool.Api.Auth;
using NotesCool.Notes.Infrastructure;
using NotesCool.Identity.Extensions;
using NotesCool.Tasks.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddShared(builder.Configuration, builder.Environment);
builder.Services.AddNotesModule(builder.Configuration);
builder.Services.AddTasksModule(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NotesCool API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();

app.MapAuthEndpoints();
app.MapIdentityEndpoints();
app.MapApiEndpoints();
app.MapTasksEndpoints();

app.Run();

public partial class Program { }