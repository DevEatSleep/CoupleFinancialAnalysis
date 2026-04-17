using CoupleChat.Models.Dto;
using CoupleChat.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoupleChat.Controllers;

/// <summary>
/// Reference data controller for INSEE household work statistics.
/// Provides a single entry point for accessing all domestic work reference data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReferenceController : ControllerBase
{
    private readonly DomestiqueReferenceService _domestiqueService;

    public ReferenceController(DomestiqueReferenceService domestiqueService)
    {
        _domestiqueService = domestiqueService;
    }

    /// <summary>
    /// Get all INSEE reference data for domestic work activities.
    /// </summary>
    /// <returns>Complete list of INSEE domestic work data</returns>
    /// <response code="200">Returns the INSEE data</response>
    [HttpGet("travail-domestique")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DomestiqueReferenceDto>>> GetTravailDomestique()
    {
        var data = await _domestiqueService.GetAllAsync();
        return Ok(data);
    }

    /// <summary>
    /// Get reference data filtered by gender (sexe).
    /// </summary>
    /// <param name="sexe">femme or homme</param>
    /// <returns>Data filtered by gender</returns>
    /// <response code="200">Returns the matching data</response>
    /// <response code="400">Invalid gender</response>
    /// <response code="404">No data found for this gender</response>
    [HttpGet("travail-domestique/sexe/{sexe}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<DomestiqueReferenceDto>>> GetByGender(string sexe)
    {
        try
        {
            var result = await _domestiqueService.GetBySexeAsync(sexe);
            if (!result.Any())
                return NotFound($"No data found for gender: {sexe}");
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get reference data filtered by activity type.
    /// </summary>
    /// <param name="activite">cuisine & ménage, soins enfants, courses, or bricolage/jardinage</param>
    /// <returns>Data filtered by activity</returns>
    /// <response code="200">Returns the matching data</response>
    /// <response code="400">Invalid activity</response>
    /// <response code="404">No data found for this activity</response>
    [HttpGet("travail-domestique/activite/{activite}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<DomestiqueReferenceDto>>> GetByActivite(string activite)
    {
        try
        {
            var result = await _domestiqueService.GetByActiviteAsync(activite);
            if (!result.Any())
                return NotFound($"No data found for activity: {activite}");
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get reference data filtered by gender and age range.
    /// </summary>
    /// <param name="sexe">femme or homme</param>
    /// <param name="tranche">18-24 ans, 25-34 ans, 35-49 ans, or 50-64 ans</param>
    /// <returns>Data filtered by gender and age range</returns>
    /// <response code="200">Returns the matching data</response>
    /// <response code="400">Invalid gender or age range</response>
    /// <response code="404">No data found for these criteria</response>
    [HttpGet("travail-domestique/sexe/{sexe}/tranche/{tranche}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<DomestiqueReferenceDto>>> GetByGenderAndAge(string sexe, string tranche)
    {
        try
        {
            var result = await _domestiqueService.GetBySexeAndAgeRangeAsync(sexe, tranche);
            if (!result.Any())
                return NotFound($"No data found for {sexe} in age range {tranche}");
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get aggregated statistics grouped by gender and activity.
    /// </summary>
    /// <returns>Statistics grouped by gender and activity</returns>
    /// <response code="200">Returns the statistics</response>
    [HttpGet("travail-domestique/statistiques")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DomestiqueStatisticsDto>>> GetStatistiques()
    {
        // Aggregate all reference data by gender and activity
        var allData = await _domestiqueService.GetAllAsync();
        
        var stats = allData
            .GroupBy(d => new { d.Sexe, d.Activite })
            .Select(g => new DomestiqueStatisticsDto
            {
                Sexe = g.Key.Sexe,
                Activite = g.Key.Activite,
                AverageMinutesPerDay = (decimal)g.Average(x => x.DureeMinutes),
                AverageHoursPerWeek = (decimal)g.Average(x => x.DureeHeures)
            })
            .OrderBy(x => x.Sexe)
            .ThenBy(x => x.Activite)
            .ToList();

        return Ok(stats);
    }
}
