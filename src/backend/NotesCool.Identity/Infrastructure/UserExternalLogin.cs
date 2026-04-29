using System;
using Microsoft.AspNetCore.Identity;

namespace NotesCool.Identity.Infrastructure;

public sealed class UserExternalLogin
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    public string ProviderKey { get; set; } = string.Empty;
    public string ProviderSubject { get; set; } = string.Empty;
    
    public string? ProviderDisplayName { get; set; }
    public string? Email { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
