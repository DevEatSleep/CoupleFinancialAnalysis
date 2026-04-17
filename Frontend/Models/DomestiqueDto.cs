namespace Frontend.Models;

/// <summary>
/// DTO for INSEE reference data on domestic work.
/// </summary>
public class DomestiqueReferenceDto
{
    public int Id { get; set; }
    public string Sexe { get; set; } = string.Empty;
    public string Activite { get; set; } = string.Empty;
    public string TrancheAge { get; set; } = string.Empty;
    public double DureeMinutes { get; set; }
    public decimal DureeHeures { get; set; }
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
