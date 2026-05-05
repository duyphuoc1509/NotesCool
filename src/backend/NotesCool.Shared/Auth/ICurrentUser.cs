using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace NotesCool.Shared.Auth;

public interface ICurrentUser
{
    string UserId { get; }
    string Role { get; }
    bool IsInRole(string role);
}

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            return principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal?.FindFirstValue("sub") ?? "anonymous";
        }
    }

    public string Role
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            return principal?.FindFirstValue(ClaimTypes.Role) ?? SystemRoles.User;
        }
    }

    public bool IsInRole(string role)
        => _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
}

public static class SystemRoles
{
    public const string User = "User";
    public const string Admin = "Admin";

    /// <summary>
    /// Maps legacy or case-variant role names (e.g. <c>admin</c> from old seed data) to
    /// <see cref="Admin"/> / <see cref="User"/> so JWTs and policies stay aligned with <c>RequireRole("Admin")</c>.
    /// </summary>
    public static string Normalize(string role)
    {
        if (string.IsNullOrEmpty(role))
        {
            return role;
        }

        if (role.Equals(Admin, StringComparison.OrdinalIgnoreCase))
        {
            return Admin;
        }

        if (role.Equals(User, StringComparison.OrdinalIgnoreCase))
        {
            return User;
        }

        return role;
    }
}
