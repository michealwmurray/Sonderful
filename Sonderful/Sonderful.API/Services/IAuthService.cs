using Sonderful.API.DTOs.Auth;

namespace Sonderful.API.Services;

/// <summary>Handles user registration and login, returning a JWT on success.</summary>
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
