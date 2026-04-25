using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using NotesCool.Notes.Application;
using NotesCool.Notes.Infrastructure;
using NotesCool.Shared.Auth;
using NotesCool.Tasks.Application;
using NotesCool.Tasks.Infrastructure;

namespace NotesCool.Api.Extensions;

public static class ServiceCollections
{
    public static IServiceCollection AddShared(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddAuthorization();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "NotesCool API",
                Version = "v1",
                Description = "API documentation for NotesCool system."
            });

            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "Please enter JWT with Bearer into field",
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddNotesModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<NotesDbContext>(o => o.UseNpgsql(config.GetConnectionString("DefaultConnection")));
        services.AddScoped<NotesService>();
        return services;
    }

    public static IServiceCollection AddTasksModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<TasksDbContext>(o => o.UseNpgsql(config.GetConnectionString("DefaultConnection")));
        services.AddScoped<TasksService>();
        return services;
    }
}
