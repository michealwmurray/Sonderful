using Sonderful.API.DTOs.Plans;
using Sonderful.API.DTOs.Users;
using Sonderful.API.Models;
using Sonderful.API.Repositories;

namespace Sonderful.API.Services;

public class PlanService : IPlanService
{
    private readonly IPlanRepository _plans;
    private readonly IRsvpRepository _rsvps;

    public PlanService(IPlanRepository plans, IRsvpRepository rsvps)
    {
        _plans = plans;
        _rsvps = rsvps;
    }

    public async Task<PlanResponse> CreatePlanAsync(int userId, CreatePlanRequest request)
    {
        var plan = new Plan
        {
            Title = request.Title,
            Description = request.Description,
            Category = request.Category,
            Capacity = request.Capacity,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            County = request.County,
            ScheduledAt = request.ScheduledAt,
            CreatorId = userId
        };

        var created = await _plans.CreateAsync(plan);
        // Re-fetch so the Creator navigation property is populated
        var full = await _plans.GetByIdAsync(created.Id);
        return MapToResponse(full!);
    }

    public async Task<PlanResponse?> GetPlanAsync(int id, int requestingUserId)
    {
        var plan = await _plans.GetByIdAsync(id);
        return plan is null ? null : MapToResponse(plan, requestingUserId);
    }

    public async Task<IEnumerable<PlanResponse>> GetNearbyPlansAsync(
        double latitude, double longitude, double radiusKm,
        PlanCategory? category, DateTime? date, int requestingUserId)
    {
        var plans = await _plans.GetNearbyAsync(latitude, longitude, radiusKm, category, date, requestingUserId);
        return plans.Select(p => MapToResponse(p, requestingUserId));
    }

    public async Task<IEnumerable<PlanResponse>> GetPlansByCountyAsync(
        string county, PlanCategory? category, DateTime? date, int requestingUserId)
    {
        var plans = await _plans.GetByCountyAsync(county, category, date, requestingUserId);
        return plans.Select(p => MapToResponse(p, requestingUserId));
    }

    public async Task<IEnumerable<PlanResponse>> GetMyPlansAsync(int userId)
    {
        var plans = await _plans.GetByCreatorAsync(userId);
        return plans.Select(p => MapToResponse(p, userId));
    }

    public async Task<PlanResponse> UpdatePlanAsync(int planId, int userId, UpdatePlanRequest request)
    {
        var plan = await _plans.GetByIdAsync(planId)
                   ?? throw new InvalidOperationException("Plan not found.");

        if (plan.CreatorId != userId)
            throw new UnauthorizedAccessException("Only the plan creator can edit this plan.");

        plan.Title = request.Title;
        plan.Description = request.Description;
        plan.Category = request.Category;
        plan.Capacity = request.Capacity;
        plan.ScheduledAt = request.ScheduledAt;

        await _plans.UpdateAsync(plan);
        var updated = await _plans.GetByIdAsync(plan.Id);
        return MapToResponse(updated!);
    }

    public async Task DeletePlanAsync(int planId, int userId)
    {
        var plan = await _plans.GetByIdAsync(planId)
                   ?? throw new InvalidOperationException("Plan not found.");

        if (plan.CreatorId != userId)
            throw new UnauthorizedAccessException("Only the plan creator can delete this plan.");

        await _plans.DeleteAsync(planId);
    }

    public async Task RsvpAsync(int planId, int userId)
    {
        var plan = await _plans.GetByIdAsync(planId)
                   ?? throw new InvalidOperationException("Plan not found.");

        var existing = await _rsvps.GetAsync(planId, userId);
        if (existing is not null)
            throw new InvalidOperationException("You have already RSVPed to this plan.");

        if (plan.RsvpCount >= plan.Capacity)
            throw new InvalidOperationException("This plan is at capacity.");

        await _rsvps.CreateAsync(new Rsvp { PlanId = planId, UserId = userId });
        plan.RsvpCount++;
        await _plans.UpdateAsync(plan);
    }

    public async Task CancelRsvpAsync(int planId, int userId)
    {
        var plan = await _plans.GetByIdAsync(planId)
                   ?? throw new InvalidOperationException("Plan not found.");

        var existing = await _rsvps.GetAsync(planId, userId);
        if (existing is null)
            throw new InvalidOperationException("You have not RSVPed to this plan.");

        await _rsvps.DeleteAsync(planId, userId);
        plan.RsvpCount--;
        await _plans.UpdateAsync(plan);
    }

    public async Task<IEnumerable<UserResponse>> GetAttendeesAsync(int planId)
    {
        var rsvps = await _rsvps.GetByPlanAsync(planId);
        return rsvps.Select(r => new UserResponse
        {
            Id = r.UserId,
            Username = r.User?.Username ?? string.Empty,
            PhotoUrl = r.User?.PhotoUrl
        });
    }

    private static PlanResponse MapToResponse(Plan plan, int requestingUserId = 0) => new()
    {
        Id = plan.Id,
        Title = plan.Title,
        Description = plan.Description,
        Category = plan.Category,
        Capacity = plan.Capacity,
        RsvpCount = plan.RsvpCount,
        IsPrivate = plan.IsPrivate,
        Latitude = plan.Latitude,
        Longitude = plan.Longitude,
        County = plan.County,
        ScheduledAt = plan.ScheduledAt,
        CreatorId = plan.CreatorId,
        CreatorUsername = plan.Creator?.Username ?? string.Empty,
        CreatorPhotoUrl = plan.Creator?.PhotoUrl,
        CreatorSonderScore = 0, // TODO: wire up once SonderScore service exists
        IsRsvped = requestingUserId > 0 && plan.Rsvps.Any(r => r.UserId == requestingUserId),
        Photos = plan.Photos.Select(p => new DTOs.Plans.PlanPhotoDto { Id = p.Id, Url = p.Url }).ToList()
    };
}
