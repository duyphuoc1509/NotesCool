using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;

namespace NotesCool.Api.Auth;

public sealed class RegistrationService(AuthDbContext dbContext)
{
    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        ValidateEmail(email);
        ValidatePassword(request.Password);

        var exists = await dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
        if (exists)
        {
            throw new DuplicateEmailException(email);
        }

        var user = new UserAccount
        {
            Email = email,
            PasswordHash = PasswordHasher.Hash(request.Password),
            Status = UserAccountStatuses.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new RegisterResponse(user.Id, user.Email, user.Status);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || email.Length > 320)
        {
            throw new RegistrationValidationException("Email is invalid.");
        }
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            throw new RegistrationValidationException("Password must be at least 8 characters long.");
        }

        if (!password.Any(char.IsLower) || !password.Any(char.IsUpper) || !password.Any(char.IsDigit) || !password.Any(c => !char.IsLetterOrDigit(c)))
        {
            throw new RegistrationValidationException("Password must include uppercase, lowercase, number, and special character.");
        }
    }
}

public sealed class DuplicateEmailException(string email) : Exception($"Email already exists: {email}");

public sealed class RegistrationValidationException(string message) : Exception(message);

public static class PasswordHasher
{
    private const int SaltSize = 128 / 8;
    private const int KeySize = 256 / 8;
    private const int Iterations = 100_000;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, Iterations, KeySize);
        return $"PBKDF2-SHA256${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
    }
}
