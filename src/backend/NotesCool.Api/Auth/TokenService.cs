using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace NotesCool.Api.Auth;

public interface IAccessTokenService
{
    LoginResponse CreateLoginResponse(RegisteredUser user);
}

public sealed class JwtAccessTokenService : IAccessTokenService
{
    private readonly IConfiguration _configuration;

    public JwtAccessTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public LoginResponse CreateLoginResponse(RegisteredUser user)
    {
        var expiresIn = _configuration.GetValue<int?>("Jwt:ExpiresInSeconds") ?? 3600;
        var expires = DateTime.UtcNow.AddSeconds(expiresIn);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetSigningKey()));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.Role, "User")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "NotesCool",
            audience: _configuration["Jwt:Audience"] ?? "NotesCool",
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new LoginResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            "Bearer",
            expiresIn,
            new AuthUserResponse(user.Id, user.Email, user.DisplayName));
    }

    private string GetSigningKey() => _configuration["Jwt:SigningKey"] ?? "NotesCool development signing key must be at least 32 bytes";
}
