using Sonderful.API.DTOs.Plans;
using Sonderful.API.DTOs.Users;
using Sonderful.API.Models;

namespace Sonderful.API.Services;

/// <summary>Handles plan CRUD, RSVP management, and attendee listing.</summary>
public interface IPlanService
{
    Task<PlanResponse> CreatePlanAsync(int userId, CreatePlanRequest request);

    /// <summary>Returns null if no plan with that ID exists.</summary>
    Task<PlanResponse?> GetPlanAsync(int id, int requestingUserId);

    Task<IEnumerable<PlanResponse>> GetNearbyPlansAsync(double latitude, double longitude, double radiusKm, PlanCategory? category, DateTime? date, int requestingUserId);

    Task<IEnumerable<PlanResponse>> GetPlansByCountyAsync(string county, PlanCategory? category, DateTime? date, int requestingUserId);

    Task<IEnumerable<PlanResponse>> GetMyPlansAsync(int userId);

    /// <summary>Only the plan creator can update. Throws if not found or not authorised.</summary>
    Task<PlanResponse> UpdatePlanAsync(int planId, int userId, UpdatePlanRequest request);

    Task DeletePlanAsync(int planId, int userId);

    Task RsvpAsync(int planId, int userId);
    Task CancelRsvpAsync(int planId, int userId);

    Task<IEnumerable<UserResponse>> GetAttendeesAsync(int planId);
}
