using System.Security.Claims;

namespace NotesCool.Shared.Auth;

public interface ICurrentUser
{
    string UserId { get; }
}

public sealed class CurrentUser : ICurrentUser
{
    private readonly ClaimsPrincipal _principal;
    public CurrentUser(ClaimsPrincipal principal) => _principal = principal;
    public string UserId => _principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? _principal.FindFirstValue("sub") ?? "anonymous";
}
