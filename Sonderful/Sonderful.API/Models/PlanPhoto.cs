namespace Sonderful.API.Models;

public class PlanPhoto
{
    public int Id { get; set; }
    public int PlanId { get; set; }
    public Plan Plan { get; set; } = null!;
    // Server-relative path, e.g. /uploads/plans/3_abc.jpg
    public string Url { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
