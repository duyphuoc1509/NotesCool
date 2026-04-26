using Microsoft.AspNetCore.Identity;

namespace NotesCool.Identity.Infrastructure;

public sealed class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public AccountStatus Status { get; set; } = AccountStatus.Active;
}
