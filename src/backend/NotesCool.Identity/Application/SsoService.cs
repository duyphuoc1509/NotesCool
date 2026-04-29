using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NotesCool.Identity.Infrastructure;

namespace NotesCool.Identity.Application;

public sealed class SsoService
{
    private readonly IdentityDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public SsoService(IdentityDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<ApplicationUser> GetOrCreateUserAsync(string provider, string providerUserId, string? email, string? displayName)
    {
        var normalizedProvider = Normalize(provider);
        var providerUserIdTrimmed = providerUserId.Trim();

        var existingLogin = await _dbContext.UserExternalLogins
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.ProviderKey == normalizedProvider && x.ProviderSubject == providerUserIdTrimmed);

        if (existingLogin is null && !string.IsNullOrWhiteSpace(email))
        {
            var userByEmail = await _userManager.FindByEmailAsync(email);
            if (userByEmail is not null)
            {
                existingLogin = await _dbContext.UserExternalLogins
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x => x.UserId == userByEmail.Id && x.ProviderKey == normalizedProvider && x.ProviderSubject == providerUserIdTrimmed);
                
                if (existingLogin is null)
                {
                    existingLogin = new UserExternalLogin
                    {
                        UserId = userByEmail.Id,
                        ProviderKey = normalizedProvider,
                        ProviderSubject = providerUserIdTrimmed,
                        ProviderDisplayName = displayName,
                        Email = email
                    };
                    _dbContext.UserExternalLogins.Add(existingLogin);
                    await _dbContext.SaveChangesAsync();
                }
                
                return userByEmail;
            }
        }

        if (existingLogin is not null)
        {
            return existingLogin.User;
        }

        var newUserId = string.IsNullOrWhiteSpace(email) ? Guid.NewGuid().ToString("N") : email.Trim().ToLowerInvariant();
        var user = new ApplicationUser
        {
            Id = newUserId,
            UserName = newUserId,
            Email = email,
            DisplayName = displayName ?? "Unknown",
            Status = AccountStatus.Active,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(x => x.Description))}");
        }

        var login = new UserExternalLogin
        {
            UserId = user.Id,
            ProviderKey = normalizedProvider,
            ProviderSubject = providerUserIdTrimmed,
            ProviderDisplayName = displayName,
            Email = email
        };
        _dbContext.UserExternalLogins.Add(login);
        await _dbContext.SaveChangesAsync();

        return user;
    }

    public async Task<ApplicationUser> LinkProviderAsync(string userId, string provider, string providerUserId, string? email, string? displayName)
    {
        var normalizedProvider = Normalize(provider);
        var providerUserIdTrimmed = providerUserId.Trim();

        var existingLogin = await _dbContext.UserExternalLogins
            .FirstOrDefaultAsync(x => x.ProviderKey == normalizedProvider && x.ProviderSubject == providerUserIdTrimmed);

        if (existingLogin is not null && !string.Equals(existingLogin.UserId, userId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Provider identity is already linked to another account.");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = userId,
                UserName = string.IsNullOrWhiteSpace(email) ? userId : email,
                Email = email,
                DisplayName = displayName ?? userId,
                Status = AccountStatus.Active,
                EmailConfirmed = !string.IsNullOrWhiteSpace(email)
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create user: {string.Join(", ", createResult.Errors.Select(x => x.Description))}");
            }
        }

        if (existingLogin is null)
        {
            var login = new UserExternalLogin
            {
                UserId = user.Id,
                ProviderKey = normalizedProvider,
                ProviderSubject = providerUserIdTrimmed,
                ProviderDisplayName = displayName,
                Email = email
            };
            _dbContext.UserExternalLogins.Add(login);
            await _dbContext.SaveChangesAsync();
        }

        return user;
    }

    public async Task<(bool Success, string? Error)> TryUnlinkProviderAsync(string userId, string provider)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return (false, "User was not found.");
        }

        var normalizedProvider = Normalize(provider);
        
        var logins = await _dbContext.UserExternalLogins
            .Where(x => x.UserId == userId)
            .ToListAsync();

        var loginToRemove = logins.FirstOrDefault(x => x.ProviderKey == normalizedProvider);
        if (loginToRemove is null)
        {
            return (false, "Provider is not linked to this account.");
        }

        if (logins.Count <= 1)
        {
            return (false, "At least one login method must remain linked.");
        }

        _dbContext.UserExternalLogins.Remove(loginToRemove);
        await _dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<IReadOnlyCollection<UserExternalLogin>> GetProvidersAsync(string userId)
    {
        return await _dbContext.UserExternalLogins
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public bool IsValidCallback(string provider, string code, string state, string providerUserId)
        => !string.IsNullOrWhiteSpace(provider)
           && !string.IsNullOrWhiteSpace(code)
           && !string.IsNullOrWhiteSpace(state)
           && !string.IsNullOrWhiteSpace(providerUserId)
           && state.StartsWith("sso_", StringComparison.OrdinalIgnoreCase);

    private static string Normalize(string provider) => provider.Trim().ToLowerInvariant();
}
