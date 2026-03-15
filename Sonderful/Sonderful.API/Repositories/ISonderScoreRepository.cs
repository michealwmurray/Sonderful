using Sonderful.API.Models;

namespace Sonderful.API.Repositories;

public interface ISonderScoreRepository
{
    Task<SonderScore> CreateAsync(SonderScore score);

    // Enforce one score per rater per user per plan
    Task<bool> ExistsAsync(int planId, int raterId, int ratedUserId);

    Task<double> GetAverageForUserAsync(int userId);
}
