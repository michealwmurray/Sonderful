namespace Sonderful.App.DTOs;

public class PublicUserProfile
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? PhotoUrl { get; set; }
    public double SonderScore { get; set; }
}
