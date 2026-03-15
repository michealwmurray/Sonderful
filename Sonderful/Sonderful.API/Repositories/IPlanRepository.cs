using Sonderful.API.Models;

namespace Sonderful.API.Repositories;

/// <summary>
/// Data access for plans, including photo management.
/// </summary>
public interface IPlanRepository
{
    Task<Plan?> GetByIdAsync(int id);

    // Returns nearby public plans within a radius, uses bounding box then Haversine
    Task<IEnumerable<Plan>> GetNearbyAsync(double latitude, double longitude, double radiusKm, PlanCategory? category, DateTime? date, int requestingUserId);

    Task<IEnumerable<Plan>> GetByCountyAsync(string county, PlanCategory? category, DateTime? date, int requestingUserId);

    // All plans a user created or RSVPed to
    Task<IEnumerable<Plan>> GetByCreatorAsync(int creatorUserId);

    Task<Plan> CreateAsync(Plan plan);
    Task<Plan> UpdateAsync(Plan plan);
    Task DeleteAsync(int id);

    Task<PlanPhoto?> GetPhotoAsync(int photoId);
    Task<PlanPhoto> AddPhotoAsync(PlanPhoto photo);
    Task DeletePhotoAsync(int photoId);
}
