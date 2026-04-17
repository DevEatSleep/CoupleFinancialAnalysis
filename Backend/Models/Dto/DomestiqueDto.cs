namespace CoupleChat.Models.Dto;

/// <summary>
/// DTO for INSEE reference data on domestic work.
/// </summary>
public class DomestiqueReferenceDto
{
    public int Id { get; set; }
    /// <summary>Gender: "femme" or "homme".</summary>
    public string Sexe { get; set; } = string.Empty;
    /// <summary>Activity type: cuisine & ménage, soins enfants, courses, bricolage/jardinage.</summary>
    public string Activite { get; set; } = string.Empty;
    /// <summary>Age range e.g., "18-24 ans".</summary>
    public string TrancheAge { get; set; } = string.Empty;
    /// <summary>Daily duration in minutes from INSEE data.</summary>
    public double DureeMinutes { get; set; }
    /// <summary>Daily duration in hours (calculated).</summary>
    public decimal DureeHeures { get; set; }
}

/// <summary>
/// DTO for domestic work response with comparison to INSEE references.
/// </summary>
public class DomestiqueResponseDto
{
    public int Id { get; set; }
    /// <summary>Person: "woman", "man", or "shared".</summary>
    public string Person { get; set; } = string.Empty;
    public string Activite { get; set; } = string.Empty;
    public decimal HeuresParSemaine { get; set; }
    public decimal ValeurMonetaire { get; set; }
    public DateTime CreatedAt { get; set; }
    
    /// <summary>INSEE reference value for woman (hours per week).</summary>
    public decimal InseeReferenceFemme { get; set; }
    /// <summary>INSEE reference value for man (hours per week).</summary>
    public decimal InseeReferenceHomme { get; set; }
}

/// <summary>
/// DTO for aggregated domestic work statistics by gender and activity.
/// </summary>
public class DomestiqueStatisticsDto
{
    public string Sexe { get; set; } = string.Empty;
    public string Activite { get; set; } = string.Empty;
    public decimal AverageMinutesPerDay { get; set; }
    public decimal AverageHoursPerWeek { get; set; }
}
