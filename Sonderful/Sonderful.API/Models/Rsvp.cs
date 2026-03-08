namespace Sonderful.API.Models;

public class Rsvp
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int PlanId { get; set; }
    public Plan Plan { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
