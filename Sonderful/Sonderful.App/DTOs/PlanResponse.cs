namespace Sonderful.App.DTOs;

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

    public string? CoverPhotoUrl => Photos.Count > 0 ? Photos[0].Url : null;
    public bool HasPhotos => Photos.Count > 0;
    public string GoingLabel => $"{RsvpCount}/{Capacity} going";
    public int SpotsLeft => Capacity - RsvpCount;
    public bool IsFull => RsvpCount >= Capacity;
}
