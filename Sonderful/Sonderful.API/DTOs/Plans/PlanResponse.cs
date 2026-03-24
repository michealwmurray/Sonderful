using Sonderful.API.Models;

namespace Sonderful.API.DTOs.Plans;

/// <summary>Flattened plan data returned by the API.</summary>
public class PlanResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PlanCategory Category { get; set; }
    public int Capacity { get; set; }
    public int RsvpCount { get; set; }
    public bool IsPrivate { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? County { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int CreatorId { get; set; }
    public string CreatorUsername { get; set; } = string.Empty;
    public string? CreatorPhotoUrl { get; set; }
    public double CreatorSonderScore { get; set; }
    public bool IsRsvped { get; set; }
    public List<PlanPhotoDto> Photos { get; set; } = new();
}
