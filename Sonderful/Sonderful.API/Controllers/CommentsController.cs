using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sonderful.API.DTOs.Comments;
using Sonderful.API.Services;

namespace Sonderful.API.Controllers;

[ApiController]
[Route("api/plans/{planId}/comments")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService) => _commentService = commentService;

    // Returns all comments on a plan, oldest first
    [HttpGet]
    public async Task<IActionResult> Get(int planId)
    {
        var comments = await _commentService.GetCommentsAsync(planId);
        return Ok(comments);
    }

    [HttpPost]
    public async Task<IActionResult> Post(int planId, AddCommentRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var comment = await _commentService.AddCommentAsync(planId, userId, request);
        return Ok(comment);
    }

    // Author or plan creator can delete, 403 for anyone else
    [HttpDelete("{commentId}")]
    public async Task<IActionResult> Delete(int planId, int commentId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            await _commentService.DeleteCommentAsync(commentId, userId);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        catch (InvalidOperationException ex) { return NotFound(ex.Message); }
    }
}
