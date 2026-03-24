using Sonderful.API.Models;

namespace Sonderful.API.DTOs.Plans;

public class UpdatePlanRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PlanCategory Category { get; set; }
    public int Capacity { get; set; }
    public DateTime ScheduledAt { get; set; }
}
