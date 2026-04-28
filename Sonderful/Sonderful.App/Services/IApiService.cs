using Sonderful.App.DTOs;

namespace Sonderful.App.Services;

public interface IApiService
{
    // Auth
    Task<AuthResponse> LoginAsync(string identifier, string password);
    Task<AuthResponse> RegisterAsync(string username, string email, string password);

    // Plans
    Task<List<PlanResponse>> GetNearbyPlansAsync(double lat, double lon, double radiusKm, string? category, DateTime? date = null);
    Task<List<PlanResponse>> GetPlansByCountyAsync(string county, string? category, DateTime? date = null);
    Task<List<PlanResponse>> GetMyPlansAsync();
    Task<PlanResponse> GetPlanAsync(int planId);
    Task<PlanResponse> CreatePlanAsync(string title, string? description, string category, int capacity, double lat, double lon, string? county, DateTime scheduledAt);
    Task<PlanResponse> UpdatePlanAsync(int planId, string title, string? description, string category, int capacity, DateTime scheduledAt);
    Task DeletePlanAsync(int planId);
    Task RsvpAsync(int planId);
    Task CancelRsvpAsync(int planId);

    // Comments
    Task<List<CommentResponse>> GetCommentsAsync(int planId);
    Task<CommentResponse> AddCommentAsync(int planId, string content);
    Task DeleteCommentAsync(int planId, int commentId);

    // Attendees
    Task<List<UserResponse>> GetAttendeesAsync(int planId);

    // Scores
    Task<double> GetUserScoreAsync(int userId);
    Task SubmitScoreAsync(int planId, int ratedUserId, int score);

    // Profile
    Task<string?> GetMyBioAsync();
    Task UpdateBioAsync(string? bio);

    // Photos
    Task<string> UploadProfilePhotoAsync(Stream photoStream, string fileName, string contentType);
    Task<(int photoId, string url)> UploadPlanPhotoAsync(int planId, Stream photoStream, string fileName, string contentType);
    Task DeletePlanPhotoAsync(int planId, int photoId);
}
