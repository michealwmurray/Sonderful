namespace Sonderful.API.Models;

public class Plan
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PlanCategory Category { get; set; }
    public int Capacity { get; set; }
    // cached count, updated on each RSVP change
    public int RsvpCount { get; set; }
    public bool IsPrivate { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? County { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CreatorId { get; set; }
    public User Creator { get; set; } = null!;
    public ICollection<Rsvp> Rsvps { get; set; } = new List<Rsvp>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<SonderScore> SonderScores { get; set; } = new List<SonderScore>();
    public ICollection<PlanPhoto> Photos { get; set; } = new List<PlanPhoto>();
}
