using Microsoft.EntityFrameworkCore;
using Sonderful.API.Data;
using Sonderful.API.Models;

namespace Sonderful.API.Repositories;

public class PlanRepository : IPlanRepository
{
    private readonly AppDbContext _context;

    public PlanRepository(AppDbContext context) => _context = context;

    public async Task<Plan?> GetByIdAsync(int id) =>
        await _context.Plans
            .Include(p => p.Creator)
            .Include(p => p.Rsvps)
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Plan>> GetNearbyAsync(
        double latitude, double longitude, double radiusKm,
        PlanCategory? category, DateTime? date, int requestingUserId)
    {
        // Bounding box to narrow down before doing Haversine
        double latDelta = radiusKm / 111.0;
        double lngDelta = radiusKm / (111.0 * Math.Cos(latitude * Math.PI / 180.0));

        var query = _context.Plans
            .Include(p => p.Creator)
            .Include(p => p.Photos)
            .Include(p => p.Rsvps)
            .Where(p =>
                !p.IsPrivate &&
                p.ScheduledAt >= DateTime.UtcNow &&
                p.Latitude >= latitude - latDelta &&
                p.Latitude <= latitude + latDelta &&
                p.Longitude >= longitude - lngDelta &&
                p.Longitude <= longitude + lngDelta &&
                (p.RsvpCount < p.Capacity
                 || p.CreatorId == requestingUserId
                 || p.Rsvps.Any(r => r.UserId == requestingUserId)));

        if (category.HasValue)
            query = query.Where(p => p.Category == category.Value);

        if (date.HasValue)
        {
            var start = date.Value.Date;
            var end = start.AddDays(1);
            query = query.Where(p => p.ScheduledAt >= start && p.ScheduledAt < end);
        }

        var plans = await query.ToListAsync();

        // Second pass with actual distance calculation
        return plans.Where(p => Haversine(latitude, longitude, p.Latitude, p.Longitude) <= radiusKm);
    }

    public async Task<IEnumerable<Plan>> GetByCountyAsync(
        string county, PlanCategory? category, DateTime? date, int requestingUserId)
    {
        var query = _context.Plans
            .Include(p => p.Creator)
            .Include(p => p.Photos)
            .Include(p => p.Rsvps)
            .Where(p =>
                !p.IsPrivate &&
                p.ScheduledAt >= DateTime.UtcNow &&
                p.County == county &&
                (p.RsvpCount < p.Capacity
                 || p.CreatorId == requestingUserId
                 || p.Rsvps.Any(r => r.UserId == requestingUserId)));

        if (category.HasValue)
            query = query.Where(p => p.Category == category.Value);

        if (date.HasValue)
        {
            var start = date.Value.Date;
            var end = start.AddDays(1);
            query = query.Where(p => p.ScheduledAt >= start && p.ScheduledAt < end);
        }

        return await query.OrderBy(p => p.ScheduledAt).ToListAsync();
    }

    public async Task<IEnumerable<Plan>> GetByCreatorAsync(int creatorUserId)
    {
        return await _context.Plans
            .Include(p => p.Creator)
            .Include(p => p.Photos)
            .Include(p => p.Rsvps)
            .Where(p => p.CreatorId == creatorUserId || p.Rsvps.Any(r => r.UserId == creatorUserId))
            .OrderBy(p => p.ScheduledAt)
            .ToListAsync();
    }

    public async Task<Plan> CreateAsync(Plan plan)
    {
        _context.Plans.Add(plan);
        await _context.SaveChangesAsync();
        return plan;
    }

    public async Task<Plan> UpdateAsync(Plan plan)
    {
        _context.Plans.Update(plan);
        await _context.SaveChangesAsync();
        return plan;
    }

    public async Task DeleteAsync(int id)
    {
        var plan = await _context.Plans.FindAsync(id);
        if (plan is not null)
        {
            _context.Plans.Remove(plan);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<PlanPhoto?> GetPhotoAsync(int photoId) =>
        await _context.PlanPhotos.FindAsync(photoId);

    public async Task<PlanPhoto> AddPhotoAsync(PlanPhoto photo)
    {
        _context.PlanPhotos.Add(photo);
        await _context.SaveChangesAsync();
        return photo;
    }

    public async Task DeletePhotoAsync(int photoId)
    {
        var photo = await _context.PlanPhotos.FindAsync(photoId);
        if (photo is not null)
        {
            _context.PlanPhotos.Remove(photo);
            await _context.SaveChangesAsync();
        }
    }

    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        double dLat = (lat2 - lat1) * Math.PI / 180.0;
        double dLon = (lon2 - lon1) * Math.PI / 180.0;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
