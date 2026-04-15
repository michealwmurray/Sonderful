using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Sonderful.API.DTOs.Auth;
using Sonderful.API.Models;
using Sonderful.API.Repositories;

namespace Sonderful.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly string _jwtKey;

    public AuthService(IUserRepository users, IConfiguration config)
    {
        _users = users;
        _jwtKey = config["Jwt:Key"]!;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existing = await _users.GetByEmailAsync(request.Email);
        if (existing is not null)
            throw new InvalidOperationException("An account with that email already exists.");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        var created = await _users.CreateAsync(user);
        return BuildResponse(created);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _users.GetByEmailAsync(request.Identifier)
                   ?? await _users.GetByUsernameAsync(request.Identifier)
                   ?? throw new UnauthorizedAccessException("Invalid email/username or password.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email/username or password.");

        return BuildResponse(user);
    }

    private AuthResponse BuildResponse(User user) => new()
    {
        Token = GenerateJwt(user),
        UserId = user.Id,
        Username = user.Username,
        PhotoUrl = user.PhotoUrl
    };

    private string GenerateJwt(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
