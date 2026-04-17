namespace CoupleChat.Models;

public class DomestiqueResponse
{
    public int Id { get; set; }
    public string Person { get; set; } = string.Empty; // "woman" or "man"
    public string Activite { get; set; } = string.Empty; // cuisine & ménage, soins enfants, courses, bricolage/jardinage
    public decimal HeuresParSemaine { get; set; } // declared hours per week by the user
    public decimal InseeRefFemme { get; set; }    // INSEE reference hours/week for women
    public decimal InseeRefHomme { get; set; }    // INSEE reference hours/week for men
    public decimal ValeurMonetaire { get; set; } // monetary value (hours × SMIC hourly rate)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
