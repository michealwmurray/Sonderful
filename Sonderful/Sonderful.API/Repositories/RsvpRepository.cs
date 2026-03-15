using Microsoft.EntityFrameworkCore;
using Sonderful.API.Data;
using Sonderful.API.Models;

namespace Sonderful.API.Repositories;

public class RsvpRepository : IRsvpRepository
{
    private readonly AppDbContext _context;

    public RsvpRepository(AppDbContext context) => _context = context;

    public async Task<Rsvp?> GetAsync(int planId, int userId) =>
        await _context.Rsvps.FirstOrDefaultAsync(r => r.PlanId == planId && r.UserId == userId);

    // Include User so we can show attendee names
    public async Task<IEnumerable<Rsvp>> GetByPlanAsync(int planId) =>
        await _context.Rsvps
            .Include(r => r.User)
            .Where(r => r.PlanId == planId)
            .ToListAsync();

    public async Task<Rsvp> CreateAsync(Rsvp rsvp)
    {
        _context.Rsvps.Add(rsvp);
        await _context.SaveChangesAsync();
        return rsvp;
    }

    public async Task DeleteAsync(int planId, int userId)
    {
        var rsvp = await GetAsync(planId, userId);
        if (rsvp is not null)
        {
            _context.Rsvps.Remove(rsvp);
            await _context.SaveChangesAsync();
        }
    }
}
