using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using NotesCool.Api.Auth;
using NotesCool.Api.Configuration;
using NotesCool.Api.Contracts;
using NotesCool.Api.Identity;
using NotesCool.Identity.Infrastructure;
using NotesCool.Notes.Application;
using NotesCool.Notes.Infrastructure;
using NotesCool.Shared.Auth;
using NotesCool.Shared.Security;
using NotesCool.Tasks.Application;
using NotesCool.Tasks.Infrastructure;
using NotesCool.Shared.Configuration;
using NotesCool.Reminders.Application;
using NotesCool.Reminders.Infrastructure;
using NotesCool.Workspaces.Application;
using NotesCool.Workspaces.Infrastructure;

namespace NotesCool.Api.Extensions;

public static class ServiceCollections
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddSingleton<IUserCredentialStore, InMemoryUserCredentialStore>();
        services.AddSingleton<IAccessTokenService, JwtAccessTokenService>();
        services.AddSingleton<SsoStore>();
        services.AddSingleton<AuthStore>();
        services.AddSingleton<IRefreshTokenStore, InMemoryRefreshTokenStore>();
        services.AddHttpClient();
        services.AddScoped<ISecurityAuditService, SecurityAuditService>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });
        
        services.AddSingleton<ISsoConfigService, SsoEnvironmentConfigService>();
        services.AddOptions<SsoOptions>()
            .Configure<ISsoConfigService>((options, ssoConfigService) =>
            {
                var configuredOptions = ssoConfigService.GetOptions();
                options.Providers.Clear();
                options.Providers.AddRange(configuredOptions.Providers);
            })
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<SsoOptions>>(_ => new SsoOptionsValidator(environment));
        
        var defaultConnectionString = configuration.GetDefaultConnectionString();

        services.AddScoped<RegistrationService>();
        services.AddDbContext<AuthDbContext>(o => o.UseNpgsql(defaultConnectionString));

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);

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
        var defaultConnectionString = config.GetDefaultConnectionString();

        services.AddDbContext<NotesDbContext>(o => o.UseNpgsql(defaultConnectionString));
        services.AddScoped<NotesService>();
        return services;
    }

    public static IServiceCollection AddTasksModule(this IServiceCollection services, IConfiguration config)
    {
        var defaultConnectionString = config.GetDefaultConnectionString();

        services.AddDbContext<TasksDbContext>(o => o.UseNpgsql(defaultConnectionString));
        services.AddScoped<TasksService>();
        services.AddScoped<ProjectsService>();
        return services;
    }

    public static IServiceCollection AddRemindersModule(this IServiceCollection services, IConfiguration config)
    {
        var defaultConnectionString = config.GetDefaultConnectionString();

        services.AddDbContext<RemindersDbContext>(o => o.UseNpgsql(defaultConnectionString));
        services.AddScoped<ReminderService>();
        return services;
    }

    public static IServiceCollection AddWorkspacesModule(this IServiceCollection services, IConfiguration config)
    {
        var defaultConnectionString = config.GetDefaultConnectionString();

        services.AddDbContext<WorkspacesDbContext>(o => o.UseNpgsql(defaultConnectionString));
        services.AddScoped<WorkspacesService>();
        return services;
    }
}