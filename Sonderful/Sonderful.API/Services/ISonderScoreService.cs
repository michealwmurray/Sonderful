namespace Sonderful.API.Services;

/// <summary>Manages the SonderScore rating system for plan attendees.</summary>
public interface ISonderScoreService
{
    /// <summary>Records a 1-5 rating from the plan creator for one of their attendees.</summary>
    Task SubmitScoreAsync(int planId, int raterId, int ratedUserId, int score);

    Task<double> GetAverageForUserAsync(int userId);
}
