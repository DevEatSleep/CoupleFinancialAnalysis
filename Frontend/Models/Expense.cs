namespace Frontend.Models;

public class Expense
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaidBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
