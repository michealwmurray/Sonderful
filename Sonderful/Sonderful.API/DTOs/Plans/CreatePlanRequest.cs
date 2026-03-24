using Sonderful.API.Models;

namespace Sonderful.API.DTOs.Plans;

public class CreatePlanRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PlanCategory Category { get; set; }
    public int Capacity { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? County { get; set; }
    public DateTime ScheduledAt { get; set; }
}
