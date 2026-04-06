namespace Sonderful.App.DTOs;

public class UserResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
}
