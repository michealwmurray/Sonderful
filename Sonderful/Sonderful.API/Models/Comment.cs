namespace Sonderful.API.Models;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int PlanId { get; set; }
    public Plan Plan { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
