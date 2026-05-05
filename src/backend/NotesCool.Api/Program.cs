using Microsoft.AspNetCore.HttpOverrides;
using NotesCool.Api.Contracts;
using NotesCool.Api.Extensions;
using NotesCool.Api.Auth;
using NotesCool.Notes.Infrastructure;
using NotesCool.Identity.Extensions;
using NotesCool.Tasks.Contracts;
using NotesCool.Api.Identity;
using NotesCool.Reminders.Contracts;
using NotesCool.Workspaces.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddShared(builder.Configuration, builder.Environment);
builder.Services.AddNotesModule(builder.Configuration);
builder.Services.AddTasksModule(builder.Configuration);
builder.Services.AddRemindersModule(builder.Configuration);
builder.Services.AddWorkspacesModule(builder.Configuration);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

var app = builder.Build();

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost,
    // NotesCool can sit behind more than one proxy (e.g. edge proxy/CDN -> nginx -> container).
    // The default ForwardLimit=1 only trusts the nearest value, which can leave Request.Scheme as
    // http even when the original client-facing scheme was https.
    ForwardLimit = null
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NotesCool API v1");
        c.RoutePrefix = "swagger";
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();

// Ensure the database schema exists for every module's DbContext before seeding
// or serving traffic. Safe to call on every startup (idempotent).
await app.Services.EnsureSchemaAsync();

// Seed default admin role and user. Integration tests replace IdentityDbContext
// with InMemory and rely on the canonical admin account being present.
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
app.MapProjectsEndpoints();
app.MapReminderEndpoints();
app.MapSsoEndpoints();
app.MapGoogleSsoEndpoints();
app.MapWorkspacesEndpoints();

app.Run();

public partial class Program { }
