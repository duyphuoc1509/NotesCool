using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NotesCool.Api.Auth;
using NotesCool.Api.Configuration;
using NotesCool.Api.Identity;
using NotesCool.Notes.Application;
using NotesCool.Notes.Infrastructure;
using NotesCool.Shared.Auth;
using NotesCool.Tasks.Application;
using NotesCool.Tasks.Infrastructure;
using NotesCool.Identity.Infrastructure;

namespace NotesCool.Api.Extensions;

public static class ServiceCollections
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration config, IHostEnvironment environment)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddSingleton<IUserCredentialStore, InMemoryUserCredentialStore>();
        services.AddSingleton<IAccessTokenService, JwtAccessTokenService>();
        services.AddSingleton<SsoStore>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = config["Jwt:Issuer"] ?? "NotesCool",
                    ValidAudience = config["Jwt:Audience"] ?? "NotesCool",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:SigningKey"] ?? "NotesCool development signing key must be at least 32 bytes"))
                };
            });

        services.AddAuthorization();

        services.AddOptions<SsoOptions>()
            .Bind(config.GetSection(SsoOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<SsoOptions>>(_ => new SsoOptionsValidator(environment));

        services.AddScoped<RegistrationService>();

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
