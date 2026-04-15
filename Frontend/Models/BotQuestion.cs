namespace Frontend.Models;

public class BotQuestion
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Person { get; set; } = string.Empty;
}
