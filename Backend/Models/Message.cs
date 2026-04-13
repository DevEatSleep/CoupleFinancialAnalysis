namespace CoupleChat.Models;

public class Message
{
    public int Id { get; set; }
    public string Sender { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string AvatarUrl { get; set; } = string.Empty;
}
