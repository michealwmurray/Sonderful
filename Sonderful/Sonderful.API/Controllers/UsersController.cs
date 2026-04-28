using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sonderful.API.DTOs.Users;
using Sonderful.API.Repositories;
using Sonderful.API.Services;

namespace Sonderful.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ISonderScoreService _scoreService;
    private readonly IUserRepository _users;
    private readonly IWebHostEnvironment _env;

    public UsersController(ISonderScoreService scoreService, IUserRepository users, IWebHostEnvironment env)
    {
        _scoreService = scoreService;
        _users = users;
        _env = env;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _users.GetByIdAsync(userId);
        if (user is null)
            return NotFound();
        return Ok(new { bio = user.Bio });
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _users.GetByIdAsync(userId);
        if (user is null)
            return NotFound();
        user.Bio = request.Bio;
        await _users.UpdateAsync(user);
        return Ok(new { bio = user.Bio });
    }

    // Returns the average SonderScore for a user
    [HttpGet("{id}/score")]
    public async Task<IActionResult> GetScore(int id)
    {
        var score = await _scoreService.GetAverageForUserAsync(id);
        return Ok(score);
    }

    // Upload or replace profile photo for the authenticated user
    [HttpPost("me/photo")]
    public async Task<IActionResult> UploadPhoto(IFormFile photo)
    {
        if (photo is null || photo.Length == 0)
            return BadRequest(new { message = "No photo provided." });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(photo.ContentType))
            return BadRequest(new { message = "Only JPEG, PNG, and WebP images are supported." });

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _users.GetByIdAsync(userId);
        if (user is null)
            return NotFound();

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "profiles");
        Directory.CreateDirectory(uploadsDir);

        // Remove old photo file if present
        if (!string.IsNullOrEmpty(user.PhotoUrl))
        {
            var oldFile = Path.Combine(_env.WebRootPath, user.PhotoUrl.TrimStart('/'));
            if (System.IO.File.Exists(oldFile))
                System.IO.File.Delete(oldFile);
        }

        var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext))
            ext = photo.ContentType == "image/png" ? ".png" : ".jpg";

        var fileName = $"{userId}_{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using (var stream = System.IO.File.Create(filePath))
            await photo.CopyToAsync(stream);

        user.PhotoUrl = $"/uploads/profiles/{fileName}";
        await _users.UpdateAsync(user);

        return Ok(new { photoUrl = user.PhotoUrl });
    }
}
