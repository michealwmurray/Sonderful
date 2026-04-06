using Sonderful.App.DTOs;

namespace Sonderful.App.Services;

/// <summary>
/// Holds the in-memory session state for the currently authenticated user.
/// </summary>
public class SessionService
{
    public string Token { get; private set; } = string.Empty;
    public int UserId { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string? PhotoUrl { get; set; }

    public bool IsLoggedIn => !string.IsNullOrEmpty(Token);

    public void SetSession(AuthResponse response)
    {
        Token = response.Token;
        UserId = response.UserId;
        Username = response.Username;
        PhotoUrl = response.PhotoUrl;
    }

    public void Clear()
    {
        Token = string.Empty;
        UserId = 0;
        Username = string.Empty;
        PhotoUrl = null;
    }
}
