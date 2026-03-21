using Microsoft.AspNetCore.Mvc;
using Sonderful.API.DTOs.Auth;
using Sonderful.API.Services;

namespace Sonderful.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    // Returns a JWT token on success, 409 if email already in use
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    // Returns a JWT token on success, 401 if credentials are invalid
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
    }
}
