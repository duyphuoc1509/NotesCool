namespace NotesCool.Api.Auth;

public interface IUserCredentialStore
{
    Task<RegisteredUser?> ValidateAsync(string email, string password, CancellationToken cancellationToken = default);
}

public sealed class InMemoryUserCredentialStore : IUserCredentialStore
{
    private readonly IReadOnlyDictionary<string, RegisteredUser> _usersByEmail;

    public InMemoryUserCredentialStore(IEnumerable<RegisteredUser>? users = null)
    {
        var configuredUsers = users ?? new[]
        {
            new RegisteredUser("demo-user", "demo@notescool.local", "Demo User", "P@ssw0rd!")
        };

        _usersByEmail = configuredUsers.ToDictionary(u => NormalizeEmail(u.Email), StringComparer.OrdinalIgnoreCase);
    }

    public Task<RegisteredUser?> ValidateAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return Task.FromResult<RegisteredUser?>(null);
        }

        return Task.FromResult(
            _usersByEmail.TryGetValue(NormalizeEmail(email), out var user) && user.Password == password
                ? user
                : null);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
