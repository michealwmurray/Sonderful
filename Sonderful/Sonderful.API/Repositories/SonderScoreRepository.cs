using Microsoft.EntityFrameworkCore;
using Sonderful.API.Data;
using Sonderful.API.Models;

namespace Sonderful.API.Repositories;

public class SonderScoreRepository : ISonderScoreRepository
{
    private readonly AppDbContext _context;

    public SonderScoreRepository(AppDbContext context) => _context = context;

    public async Task<SonderScore> CreateAsync(SonderScore score)
    {
        _context.SonderScores.Add(score);
        await _context.SaveChangesAsync();
        return score;
    }

    public async Task<bool> ExistsAsync(int planId, int raterId, int ratedUserId) =>
        await _context.SonderScores.AnyAsync(s =>
            s.PlanId == planId &&
            s.RaterId == raterId &&
            s.RatedUserId == ratedUserId);

    // Return 0 if they haven't been rated yet
    public async Task<double> GetAverageForUserAsync(int userId)
    {
        var scores = await _context.SonderScores
            .Where(s => s.RatedUserId == userId)
            .Select(s => s.Score)
            .ToListAsync();

        return scores.Count == 0 ? 0 : scores.Average();
    }
}
