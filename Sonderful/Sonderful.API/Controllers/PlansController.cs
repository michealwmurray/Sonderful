using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sonderful.API.DTOs.Plans;
using Sonderful.API.Models;
using Sonderful.API.Repositories;
using Sonderful.API.Services;

namespace Sonderful.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlansController : ControllerBase
{
    private readonly IPlanService _planService;
    private readonly IPlanRepository _planRepo;
    private readonly IWebHostEnvironment _env;

    public PlansController(IPlanService planService, IPlanRepository planRepo, IWebHostEnvironment env)
    {
        _planService = planService;
        _planRepo = planRepo;
        _env = env;
    }

    // Creates a new plan, returns 201 with location header
    [HttpPost]
    public async Task<IActionResult> Create(CreatePlanRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var plan = await _planService.CreatePlanAsync(userId, request);
        return CreatedAtAction(nameof(Get), new { id = plan.Id }, plan);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var plan = await _planService.GetPlanAsync(id, userId);
        return plan is null ? NotFound() : Ok(plan);
    }

    // Search by county name or lat/lng coordinates
    [HttpGet]
    public async Task<IActionResult> GetNearby(
        [FromQuery] double? lat = null,
        [FromQuery] double? lng = null,
        [FromQuery] double radius = 10,
        [FromQuery] string? county = null,
        [FromQuery] PlanCategory? category = null,
        [FromQuery] DateTime? date = null)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (!string.IsNullOrEmpty(county))
        {
            var countyPlans = await _planService.GetPlansByCountyAsync(county, category, date, userId);
            return Ok(countyPlans);
        }

        if (lat is null || lng is null)
            return BadRequest(new { message = "Provide either a county or lat/lng coordinates." });

        var nearbyPlans = await _planService.GetNearbyPlansAsync(lat.Value, lng.Value, radius, category, date, userId);
        return Ok(nearbyPlans);
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var plans = await _planService.GetMyPlansAsync(userId);
        return Ok(plans);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdatePlanRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var plan = await _planService.UpdatePlanAsync(id, userId, request);
            return Ok(plan);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return NotFound(ex.Message); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            await _planService.DeletePlanAsync(id, userId);
            return NoContent();
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return NotFound(ex.Message); }
    }

    [HttpGet("{id}/attendees")]
    public async Task<IActionResult> GetAttendees(int id)
    {
        var attendees = await _planService.GetAttendeesAsync(id);
        return Ok(attendees);
    }

    [HttpPost("{id}/rsvp")]
    public async Task<IActionResult> Rsvp(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            await _planService.RsvpAsync(id, userId);
            return NoContent();
        }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpDelete("{id}/rsvp")]
    public async Task<IActionResult> CancelRsvp(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            await _planService.CancelRsvpAsync(id, userId);
            return NoContent();
        }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPost("{id}/photos")]
    public async Task<IActionResult> UploadPhoto(int id, IFormFile photo)
    {
        if (photo is null || photo.Length == 0)
            return BadRequest(new { message = "No photo provided." });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(photo.ContentType))
            return BadRequest(new { message = "Only JPEG, PNG, and WebP images are supported." });

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var plan = await _planRepo.GetByIdAsync(id);
        if (plan is null)
            return NotFound(new { message = "Plan not found." });
        if (plan.CreatorId != userId)
            return Forbid();

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "plans");
        Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext))
            ext = photo.ContentType == "image/png" ? ".png" : ".jpg";

        var fileName = $"{id}_{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using (var stream = System.IO.File.Create(filePath))
            await photo.CopyToAsync(stream);

        var planPhoto = await _planRepo.AddPhotoAsync(new PlanPhoto
        {
            PlanId = id,
            Url = $"/uploads/plans/{fileName}"
        });

        return Ok(new { photoId = planPhoto.Id, url = planPhoto.Url });
    }

    [HttpDelete("{id}/photos/{photoId}")]
    public async Task<IActionResult> DeletePhoto(int id, int photoId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var plan = await _planRepo.GetByIdAsync(id);
        if (plan is null)
            return NotFound(new { message = "Plan not found." });
        if (plan.CreatorId != userId)
            return Forbid();

        var photo = await _planRepo.GetPhotoAsync(photoId);
        if (photo is null || photo.PlanId != id)
            return NotFound(new { message = "Photo not found." });

        var filePath = Path.Combine(_env.WebRootPath, photo.Url.TrimStart('/'));
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);

        await _planRepo.DeletePhotoAsync(photoId);
        return NoContent();
    }
}
