namespace CoupleChat.Models;

public class TravailDomestique
{
    public int Id { get; set; }
    public string Sexe { get; set; } = string.Empty; // "femme" ou "homme"
    public string Activite { get; set; } = string.Empty; // cuisine & ménage, soins enfants, courses, bricolage/jardinage
    public string TrancheAge { get; set; } = string.Empty; // 18-24 ans, 25-34 ans, 35-49 ans, 50-64 ans
    public int DureeMinutes { get; set; } // minutes par jour (données INSEE)
    public decimal DureeHeures { get; set; } // heures (calculé)
    public decimal CoutJour { get; set; } // coût estimé par jour
}
