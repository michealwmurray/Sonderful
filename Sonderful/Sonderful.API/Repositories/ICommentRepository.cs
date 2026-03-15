using Sonderful.API.Models;

namespace Sonderful.API.Repositories;

public interface ICommentRepository
{
    /// <summary>Returns comments for a plan, sorted oldest first.</summary>
    Task<IEnumerable<Comment>> GetByPlanAsync(int planId);
    Task<Comment?> GetByIdAsync(int commentId);
    Task<Comment> CreateAsync(Comment comment);
    Task DeleteAsync(int commentId);
}
