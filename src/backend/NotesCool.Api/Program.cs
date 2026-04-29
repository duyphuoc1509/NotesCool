using NotesCool.Api.Contracts;
using NotesCool.Api.Extensions;
using NotesCool.Api.Auth;
using NotesCool.Notes.Infrastructure;
using NotesCool.Identity.Extensions;
using NotesCool.Tasks.Contracts;
using NotesCool.Api.Identity;

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
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();

// Ensure the database schema exists for every module's DbContext before seeding
// or serving traffic. Safe to call on every startup (idempotent).
await app.Services.EnsureSchemaAsync();

// Seed default admin role and user
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<NotesCool.Identity.Extensions.IdentityDataSeeder>();
    await seeder.SeedAsync();
}

app.MapAuthEndpoints();
app.MapRegistrationEndpoints();
app.MapIdentityEndpoints();
app.MapApiEndpoints();
app.MapTasksEndpoints();
app.MapSsoEndpoints();
app.MapGoogleSsoEndpoints();

app.Run();

public partial class Program { }
