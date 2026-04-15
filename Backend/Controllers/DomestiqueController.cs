using CoupleChat.Data;
using CoupleChat.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoupleChat.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DomestiqueController : ControllerBase
{
    private readonly ChatDbContext _context;

    public DomestiqueController(ChatDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Récupère toutes les réponses de travail domestique
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<DomestiqueResponse>> GetAll()
    {
        return Ok(_context.DomestiqueResponses.ToList());
    }

    /// <summary>
    /// Enregistre la déclaration d'heures domestiques d'une personne pour une activité
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<DomestiqueResponse> Create([FromBody] DomestiqueResponse response)
    {
        if (string.IsNullOrWhiteSpace(response.Person) || string.IsNullOrWhiteSpace(response.Activite))
            return BadRequest("Person and Activite are required.");

        response.CreatedAt = DateTime.UtcNow;
        _context.DomestiqueResponses.Add(response);
        _context.SaveChanges();
        return CreatedAtAction(nameof(GetAll), new { id = response.Id }, response);
    }

    /// <summary>
    /// Supprime une réponse de travail domestique
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        var item = _context.DomestiqueResponses.Find(id);
        if (item == null) return NotFound();
        _context.DomestiqueResponses.Remove(item);
        _context.SaveChanges();
        return NoContent();
    }
}
