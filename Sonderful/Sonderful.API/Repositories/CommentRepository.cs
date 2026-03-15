using Microsoft.EntityFrameworkCore;
using Sonderful.API.Data;
using Sonderful.API.Models;

namespace Sonderful.API.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _context;

    public CommentRepository(AppDbContext context) => _context = context;

    // Oldest first so the conversation reads top to bottom
    public async Task<IEnumerable<Comment>> GetByPlanAsync(int planId) =>
        await _context.Comments
            .Include(c => c.User)
            .Where(c => c.PlanId == planId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

    public async Task<Comment?> GetByIdAsync(int commentId) =>
        await _context.Comments.FindAsync(commentId);

    public async Task<Comment> CreateAsync(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task DeleteAsync(int commentId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment is not null)
        {
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }
    }
}
