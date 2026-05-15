namespace CoupleChat.Models;

public class Couple
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<User> Users { get; set; } = [];
    public ICollection<Message> Messages { get; set; } = [];
    public ICollection<Response> Responses { get; set; } = [];
    public ICollection<Expense> Expenses { get; set; } = [];
    public ICollection<TravailDomestique> TravailDomestiques { get; set; } = [];
    public ICollection<DomestiqueResponse> DomestiqueResponses { get; set; } = [];
}
