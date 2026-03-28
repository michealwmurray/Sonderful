using Sonderful.API.DTOs.Comments;
using Sonderful.API.Models;
using Sonderful.API.Repositories;

namespace Sonderful.API.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _comments;
    private readonly IUserRepository _users;
    private readonly IPlanRepository _plans;

    public CommentService(ICommentRepository comments, IUserRepository users, IPlanRepository plans)
    {
        _comments = comments;
        _users = users;
        _plans = plans;
    }

    public async Task<IEnumerable<CommentResponse>> GetCommentsAsync(int planId)
    {
        var comments = await _comments.GetByPlanAsync(planId);
        return comments.Select(MapToResponse);
    }

    public async Task<CommentResponse> AddCommentAsync(int planId, int userId, AddCommentRequest request)
    {
        var comment = new Comment
        {
            Content = request.Content,
            PlanId = planId,
            UserId = userId
        };

        var created = await _comments.CreateAsync(comment);

        // CreateAsync doesn't include User - fetch separately for the response
        var user = await _users.GetByIdAsync(userId);
        created.User = user!;

        return MapToResponse(created);
    }

    public async Task DeleteCommentAsync(int commentId, int requestingUserId)
    {
        var comment = await _comments.GetByIdAsync(commentId)
                      ?? throw new InvalidOperationException("Comment not found.");

        var plan = await _plans.GetByIdAsync(comment.PlanId);
        var isPlanCreator = plan?.CreatorId == requestingUserId;

        if (comment.UserId != requestingUserId && !isPlanCreator)
            throw new UnauthorizedAccessException("You cannot delete this comment.");

        await _comments.DeleteAsync(commentId);
    }

    private static CommentResponse MapToResponse(Comment comment) => new()
    {
        Id = comment.Id,
        UserId = comment.UserId,
        Content = comment.Content,
        Username = comment.User?.Username ?? string.Empty,
        CreatedAt = comment.CreatedAt
    };
}
