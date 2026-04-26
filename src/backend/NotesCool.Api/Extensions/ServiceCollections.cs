using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NotesCool.Api.Auth;
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
        services.AddSingleton<AuthStore>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "NotesCool API",
                Version = "v1",
                Description = "API documentation for NotesCool system."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter JWT with Bearer into field. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
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
