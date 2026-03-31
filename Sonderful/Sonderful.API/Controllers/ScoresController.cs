using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sonderful.API.DTOs.Scores;
using Sonderful.API.Services;

namespace Sonderful.API.Controllers;

[ApiController]
[Route("api/plans/{planId}/scores")]
[Authorize]
public class ScoresController : ControllerBase
{
    private readonly ISonderScoreService _scoreService;

    public ScoresController(ISonderScoreService scoreService) => _scoreService = scoreService;

    // Plan creator rates an attendee 1-5 after the event
    [HttpPost]
    public async Task<IActionResult> Post(int planId, SubmitScoreRequest request)
    {
        var raterId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _scoreService.SubmitScoreAsync(planId, raterId, request.RatedUserId, request.Score);
        return NoContent();
    }
}
