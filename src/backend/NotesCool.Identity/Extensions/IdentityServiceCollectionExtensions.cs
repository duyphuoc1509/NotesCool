using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NotesCool.Identity.Application;
using NotesCool.Identity.Application.Abstractions;
using NotesCool.Identity.Infrastructure;
using NotesCool.Shared.Configuration;

namespace NotesCool.Identity.Extensions;

public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        var defaultConnectionString = configuration.GetDefaultConnectionString();

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(defaultConnectionString));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
            })
            .AddRoles<IdentityRole>()
            .AddSignInManager<SignInManager<ApplicationUser>>()
            .AddEntityFrameworkStores<IdentityDbContext>();

        services.AddScoped<AccountService>();
        services.AddScoped<SsoService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IdentityDataSeeder>();

        services.Configure<MicrosoftSsoOptions>(options =>
        {
            configuration.GetSection(MicrosoftSsoOptions.SectionName).Bind(options);
            options.TenantId = configuration["AUTH_MICROSOFT_TENANT_ID"]
                ?? options.TenantId
                ?? "common";
            options.ClientId = configuration["AUTH_MICROSOFT_CLIENT_ID"] ?? options.ClientId;
            options.ClientSecret = configuration["AUTH_MICROSOFT_CLIENT_SECRET"] ?? options.ClientSecret;
        });
        services.AddHttpClient<SsoService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
                };

                // .NET 8 validates JWTs with JsonWebTokenHandler; role claims may appear as "role" or as
                // ClaimTypes.Role depending on mapping. RequireRole("Admin") uses ClaimsPrincipal.IsInRole,
                // which only honors ClaimTypes.Role — normalize so admin endpoints accept tokens from all login paths.
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        if (context.Principal?.Identity is not ClaimsIdentity identity)
                        {
                            return Task.CompletedTask;
                        }

                        static bool IsRoleClaimType(string claimType)
                        {
                            if (string.Equals(claimType, ClaimTypes.Role, StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }

                            if (string.Equals(claimType, "role", StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }

                            return claimType.EndsWith("/role", StringComparison.OrdinalIgnoreCase);
                        }

                        var already = new HashSet<string>(
                            identity.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value),
                            StringComparer.OrdinalIgnoreCase);

                        foreach (var claim in context.Principal.Claims.Where(c => IsRoleClaimType(c.Type)))
                        {
                            if (already.Add(claim.Value))
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Role, claim.Value));
                            }
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
