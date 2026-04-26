using Microsoft.AspNetCore.Identity;
using NotesCool.Identity.Application.Abstractions;
using NotesCool.Identity.Contracts;
using NotesCool.Identity.Infrastructure;

namespace NotesCool.Identity.Application;

public sealed class AccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
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

        return _jwtTokenGenerator.CreateToken(user);
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

        return _jwtTokenGenerator.CreateToken(user);
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
