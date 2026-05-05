using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NotesCool.Identity.Application.Abstractions;
using NotesCool.Identity.Contracts;
using NotesCool.Identity.Infrastructure;
using NotesCool.Shared.Auth;

namespace NotesCool.Identity.Application;

public sealed class AccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            throw new InvalidOperationException("An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            Status = AccountStatus.Active,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }

        await EnsureRoleExistsAsync(SystemRoles.User);

        var roleResult = await _userManager.AddToRoleAsync(user, SystemRoles.User);
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", roleResult.Errors.Select(x => x.Description)));
        }

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (user.Status != AccountStatus.Active)
        {
            throw new AccountInactiveException(user.Status);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        return await BuildAuthResponseAsync(user);
    }

    public async Task<IReadOnlyList<UserManagementResponse>> GetUsersAsync()
    {
        var users = await _userManager.Users
            .OrderBy(user => user.Email)
            .ToListAsync();

        var responses = new List<UserManagementResponse>(users.Count);
        foreach (var user in users)
        {
            responses.Add(await MapUserAsync(user));
        }

        return responses;
    }

    public async Task<UserManagementResponse> UpdateUserStatusAsync(string userId, UpdateUserStatusRequest request, string actingUserId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User '{userId}' was not found.");

        if (string.Equals(user.Id, actingUserId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Admin users cannot change their own status.");
        }

        var targetRoles = await _userManager.GetRolesAsync(user);
        if (targetRoles.Contains(SystemRoles.Admin, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Admin users cannot change another admin account status.");
        }

        if (!Enum.TryParse<AccountStatus>(request.Status, ignoreCase: true, out var status))
        {
            throw new InvalidOperationException($"Unsupported account status '{request.Status}'.");
        }

        user.Status = status;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", updateResult.Errors.Select(x => x.Description)));
        }

        return await MapUserAsync(user, targetRoles);
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return _jwtTokenGenerator.CreateToken(user, roles);
    }

    private async Task<UserManagementResponse> MapUserAsync(ApplicationUser user, IEnumerable<string>? roles = null)
    {
        var resolvedRoles = (roles?.ToArray() ?? (await _userManager.GetRolesAsync(user)).ToArray())
            .Select(SystemRoles.Normalize)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return new UserManagementResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName,
            user.Status.ToString(),
            resolvedRoles);
    }

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        if (await _roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }
    }
}

public sealed class AccountInactiveException : Exception
{
    public AccountInactiveException(AccountStatus status)
        : base($"Account is not active. Current status: {status}.")
    {
        Status = status;
    }

    public AccountStatus Status { get; }
}
