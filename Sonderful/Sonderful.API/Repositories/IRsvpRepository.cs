using Sonderful.API.Models;

namespace Sonderful.API.Repositories;

public interface IRsvpRepository
{
    // Lookup by composite key (plan + user)
    Task<Rsvp?> GetAsync(int planId, int userId);
    Task<IEnumerable<Rsvp>> GetByPlanAsync(int planId);
    Task<Rsvp> CreateAsync(Rsvp rsvp);
    Task DeleteAsync(int planId, int userId);
}
