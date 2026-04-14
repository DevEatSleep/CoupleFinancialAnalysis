namespace BlazorFrontend.Models;

public class BotResponseDto
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string UserResponse { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ExtractedTag { get; set; } = string.Empty;
    public string Person { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
