namespace Sonderful.API.DTOs.Comments;

public class CommentResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
