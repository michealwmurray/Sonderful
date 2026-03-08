namespace Sonderful.API.Models;

public class SonderScore
{
    public int Id { get; set; }
    // 1–5 rating
    public int Score { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int PlanId { get; set; }
    public Plan Plan { get; set; } = null!;

    public int RaterId { get; set; }
    public User Rater { get; set; } = null!;

    public int RatedUserId { get; set; }
    public User RatedUser { get; set; } = null!;
}
