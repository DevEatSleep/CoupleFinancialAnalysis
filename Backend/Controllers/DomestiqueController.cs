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
        return Ok(_context.DomestiqueResponses.ToList());
    }

    /// <summary>
    /// Save a user's declaration of hours spent on a domestic work activity.
    /// Automatically calculates monetary value based on SMIC and compares with INSEE references.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DomestiqueResponseDto>> Create([FromBody] DomestiqueResponse response)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(response.Person) || string.IsNullOrWhiteSpace(response.Activite))
            return BadRequest("Person and Activite are required.");

        if (!SharedConstants.PersonTypes.All.Contains(response.Person))
            return BadRequest($"Invalid Person value. Must be one of: {string.Join(", ", SharedConstants.PersonTypes.All)}");

        if (!SharedConstants.Domestique.Activities.All.Contains(response.Activite))
            return BadRequest($"Invalid Activite. Must be one of: {string.Join(", ", SharedConstants.Domestique.Activities.All)}");

        // Set timestamp and calculate monetary value
        response.CreatedAt = DateTime.UtcNow;
        response.ValeurMonetaire = response.HeuresParSemaine * SharedConstants.Domestique.WeekToMonthFactor * SharedConstants.Domestique.HourlyRate;
        
        // Get INSEE reference values for comparison
        var referenceFemme = await _referenceService.GetActivityReferenceForGenderAsync(response.Activite, SharedConstants.Domestique.Sexe.Femme);
        var referenceHomme = await _referenceService.GetActivityReferenceForGenderAsync(response.Activite, SharedConstants.Domestique.Sexe.Homme);

        response.InseeRefFemme = (decimal)(referenceFemme ?? 0);
        response.InseeRefHomme = (decimal)(referenceHomme ?? 0);

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
