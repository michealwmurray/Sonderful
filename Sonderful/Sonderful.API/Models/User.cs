namespace Sonderful.API.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    // BCrypt hash — plaintext is never stored
    public string PasswordHash { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Plan> CreatedPlans { get; set; } = new List<Plan>();
    public ICollection<Rsvp> Rsvps { get; set; } = new List<Rsvp>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<SonderScore> SonderScoresReceived { get; set; } = new List<SonderScore>();
    public ICollection<SonderScore> SonderScoresGiven { get; set; } = new List<SonderScore>();
}
