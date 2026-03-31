using Sonderful.API.Models;
using Sonderful.API.Repositories;

namespace Sonderful.API.Services;

public class SonderScoreService : ISonderScoreService
{
    private readonly ISonderScoreRepository _scores;
    private readonly IPlanRepository _plans;
    private readonly IRsvpRepository _rsvps;

    public SonderScoreService(ISonderScoreRepository scores, IPlanRepository plans, IRsvpRepository rsvps)
    {
        _scores = scores;
        _plans = plans;
        _rsvps = rsvps;
    }

    public async Task SubmitScoreAsync(int planId, int raterId, int ratedUserId, int score)
    {
        if (raterId == ratedUserId)
            throw new InvalidOperationException("You cannot rate yourself.");

        if (score < 1 || score > 5)
            throw new ArgumentOutOfRangeException(nameof(score), "Score must be between 1 and 5.");

        var plan = await _plans.GetByIdAsync(planId)
                   ?? throw new InvalidOperationException("Plan not found.");

        if (plan.CreatorId != raterId)
            throw new UnauthorizedAccessException("Only the plan creator can rate attendees.");

        if (plan.ScheduledAt > DateTime.UtcNow)
            throw new InvalidOperationException("You can only rate attendees after the plan has taken place.");

        var rsvp = await _rsvps.GetAsync(planId, ratedUserId);
        if (rsvp is null)
            throw new InvalidOperationException("That user did not attend this plan.");

        var exists = await _scores.ExistsAsync(planId, raterId, ratedUserId);
        if (exists)
            throw new InvalidOperationException("You have already submitted a score for this user on this plan.");

        await _scores.CreateAsync(new SonderScore
        {
            PlanId = planId,
            RaterId = raterId,
            RatedUserId = ratedUserId,
            Score = score
        });
    }

    public Task<double> GetAverageForUserAsync(int userId) =>
        _scores.GetAverageForUserAsync(userId);
}
