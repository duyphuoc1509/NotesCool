using NotesCool.Identity.Contracts;
using NotesCool.Identity.Infrastructure;

namespace NotesCool.Identity.Application.Abstractions;

public interface IJwtTokenGenerator
{
    AuthResponse CreateToken(ApplicationUser user);
}
