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
}
