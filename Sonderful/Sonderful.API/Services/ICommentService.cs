using Sonderful.API.DTOs.Comments;

namespace Sonderful.API.Services;

/// <summary>Business logic for reading and moderating plan comments.</summary>
public interface ICommentService
{
    Task<IEnumerable<CommentResponse>> GetCommentsAsync(int planId);
    Task<CommentResponse> AddCommentAsync(int planId, int userId, AddCommentRequest request);

    /// <summary>Both the comment author and the plan creator can delete.</summary>
    Task DeleteCommentAsync(int commentId, int requestingUserId);
}
