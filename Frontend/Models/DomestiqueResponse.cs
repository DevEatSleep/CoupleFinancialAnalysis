namespace Frontend.Models;

public class DomestiqueResponse
{
    public int Id { get; set; }
    public string Person { get; set; } = string.Empty;        // "woman" or "man"
    public string Activite { get; set; } = string.Empty;      // activity key
    public decimal HeuresParSemaine { get; set; }             // user declared hours/week
    public decimal InseeRefFemme { get; set; }                // INSEE reference for women
    public decimal InseeRefHomme { get; set; }                // INSEE reference for men
    public DateTime CreatedAt { get; set; }
}
