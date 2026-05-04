using CoupleChat;
using CoupleChat.Data;
using CoupleChat.Models;
using CoupleChat.Models.Dto;
using CoupleChat.Services;
using CoupleChat.Utilities;
using Microsoft.AspNetCore.Mvc;
using Shared;
using SharedConstants = Shared.Constants;

namespace CoupleChat.Controllers;

/// <summary>
/// Controller for managing user-declared domestic work activities and comparing them with INSEE references.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DomestiqueController : ControllerBase
{
    private readonly ChatDbContext _context;
    private readonly DomestiqueReferenceService _referenceService;

    public DomestiqueController(ChatDbContext context, DomestiqueReferenceService referenceService)
    {
        _context = context;
        _referenceService = referenceService;
    }

    /// <summary>
    /// Get all user-declared domestic work responses.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<DomestiqueResponse>> GetAll()
    {
        Console.WriteLine("[DomestiqueController] GET /api/domestique called");
        var items = _context.DomestiqueResponses.ToList();
        Console.WriteLine($"[DomestiqueController] Retrieved {items.Count} DomestiqueResponses");
        foreach (var item in items)
            item.ValeurMonetaire = Math.Round(item.ValeurMonetaire, 2);
        return Ok(items);
    }

    /// <summary>
    /// Save a user's declaration of hours spent on a domestic work activity.
    /// Upserts: updates existing entry for the same person+activity, or creates a new one.
    /// Automatically calculates monetary value based on SMIC and compares with INSEE references.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DomestiqueResponseDto>> Create([FromBody] DomestiqueResponse response)
    {
        Console.WriteLine($"[DomestiqueController] POST /api/domestique called with Person={(response?.Person ?? "(null)" )}, Activite={(response?.Activite ?? "(null)" )}, HeuresParSemaine={(response != null ? response.HeuresParSemaine.ToString() : "(null)")}");
        // Validate input
        if (response == null || string.IsNullOrWhiteSpace(response.Person) || string.IsNullOrWhiteSpace(response.Activite))
            return BadRequest("Person and Activite are required.");

        if (!SharedConstants.PersonTypes.All.Contains(response.Person))
            return BadRequest($"Invalid Person value. Must be one of: {string.Join(", ", SharedConstants.PersonTypes.All)}");

        if (!SharedConstants.Domestique.Activities.All.Contains(response.Activite))
            return BadRequest($"Invalid Activite. Must be one of: {string.Join(", ", SharedConstants.Domestique.Activities.All)}");

        // Get INSEE reference values for comparison.
        // GetActivityReferenceForGenderAsync returns hours/DAY (averaged across age ranges).
        // The user input (HeuresParSemaine) and the display layer expect hours/WEEK,
        // so convert daily → weekly here (× 7) at the single boundary.
        var referenceFemmeDay = await _referenceService.GetActivityReferenceForGenderAsync(response.Activite, SharedConstants.Domestique.Sexe.Femme);
        var referenceHommeDay = await _referenceService.GetActivityReferenceForGenderAsync(response.Activite, SharedConstants.Domestique.Sexe.Homme);
        var referenceFemme = referenceFemmeDay.HasValue ? referenceFemmeDay * 7m : null;
        var referenceHomme = referenceHommeDay.HasValue ? referenceHommeDay * 7m : null;

        // Upsert: update existing record if one exists for same person + activity
        var existing = _context.DomestiqueResponses
            .FirstOrDefault(r => r.Person == response.Person && r.Activite == response.Activite);

        if (existing is not null)
        {
            existing.HeuresParSemaine = response.HeuresParSemaine;
            existing.ValeurMonetaire = Math.Round(response.HeuresParSemaine * SharedConstants.Domestique.WeekToMonthFactor * SharedConstants.Domestique.HourlyRate, 2);
            existing.InseeRefFemme = referenceFemme ?? 0;
            existing.InseeRefHomme = referenceHomme ?? 0;
            existing.CreatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var updatedDto = new DomestiqueResponseDto
            {
                Id = existing.Id,
                Person = existing.Person,
                Activite = existing.Activite,
                HeuresParSemaine = existing.HeuresParSemaine,
                ValeurMonetaire = existing.ValeurMonetaire,
                CreatedAt = existing.CreatedAt,
                InseeReferenceFemme = referenceFemme ?? 0,
                InseeReferenceHomme = referenceHomme ?? 0
            };
            Console.WriteLine($"[DomestiqueController] Updated DomestiqueResponse id={existing.Id} Person={existing.Person} Activite={existing.Activite} HeuresParSemaine={existing.HeuresParSemaine}");
            return CreatedAtAction(nameof(GetAll), new { id = existing.Id }, updatedDto);
        }

        // New entry
        response.CreatedAt = DateTime.UtcNow;
        response.ValeurMonetaire = Math.Round(response.HeuresParSemaine * SharedConstants.Domestique.WeekToMonthFactor * SharedConstants.Domestique.HourlyRate, 2);
        response.InseeRefFemme = referenceFemme ?? 0;
        response.InseeRefHomme = referenceHomme ?? 0;

        _context.DomestiqueResponses.Add(response);
        await _context.SaveChangesAsync();

        // Return DTO with reference data
        var responseDto = new DomestiqueResponseDto
        {
            Id = response.Id,
            Person = response.Person,
            Activite = response.Activite,
            HeuresParSemaine = response.HeuresParSemaine,
            ValeurMonetaire = response.ValeurMonetaire,
            CreatedAt = response.CreatedAt,
            InseeReferenceFemme = referenceFemme ?? 0,
            InseeReferenceHomme = referenceHomme ?? 0
        };

        Console.WriteLine($"[DomestiqueController] Created DomestiqueResponse id={response.Id} Person={response.Person} Activite={response.Activite} HeuresParSemaine={response.HeuresParSemaine}");
        return CreatedAtAction(nameof(GetAll), new { id = response.Id }, responseDto);
    }

    /// <summary>
    /// Delete a domestic work response.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.DomestiqueResponses.FindAsync(id);
        if (item == null) return NotFound();
        
        _context.DomestiqueResponses.Remove(item);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
