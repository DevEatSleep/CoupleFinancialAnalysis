namespace CoupleChat.Models;

public class Expense
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaidBy { get; set; } = string.Empty; // "woman", "man", or "shared"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
