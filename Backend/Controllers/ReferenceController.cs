using CoupleChat.Data;
using CoupleChat.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoupleChat.Controllers;

/// <summary>
/// Reference data controller for INSEE household work statistics
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReferenceController : ControllerBase
{
    private readonly ChatDbContext _context;

    public ReferenceController(ChatDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Récupère toutes les données INSEE du travail domestique
    /// </summary>
    /// <returns>Liste complète des données de travail domestique</returns>
    /// <response code="200">Retourne la liste des données INSEE</response>
    [HttpGet("travail-domestique")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<TravailDomestique>> GetTravailDomestique()
    {
        return Ok(_context.TravailDomestique.ToList());
    }

    /// <summary>
    /// Filtre les données par sexe (femme/homme)
    /// </summary>
    /// <param name="sexe">femme ou homme</param>
    /// <returns>Données filtrées par sexe</returns>
    /// <response code="200">Retourne les données correspondantes</response>
    /// <response code="404">Aucune donnée trouvée pour ce sexe</response>
    [HttpGet("travail-domestique/sexe/{sexe}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<IEnumerable<TravailDomestique>> GetByGender(string sexe)
    {
        var result = _context.TravailDomestique
            .Where(t => t.Sexe.ToLower() == sexe.ToLower())
            .ToList();
        
        if (!result.Any())
            return NotFound($"Aucune donnée trouvée pour le sexe: {sexe}");
        
        return Ok(result);
    }

    /// <summary>
    /// Filtre les données par type d'activité
    /// </summary>
    /// <param name="activite">cuisine & ménage, soins enfants, courses, bricolage/jardinage</param>
    /// <returns>Données filtrées par activité</returns>
    /// <response code="200">Retourne les données correspondantes</response>
    /// <response code="404">Aucune donnée trouvée pour cette activité</response>
    [HttpGet("travail-domestique/activite/{activite}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<IEnumerable<TravailDomestique>> GetByActivite(string activite)
    {
        var result = _context.TravailDomestique
            .Where(t => t.Activite.Contains(activite, StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        if (!result.Any())
            return NotFound($"Aucune donnée trouvée pour l'activité: {activite}");
        
        return Ok(result);
    }

    /// <summary>
    /// Filtre les données par sexe et tranche d'âge
    /// </summary>
    /// <param name="sexe">femme ou homme</param>
    /// <param name="tranche">18-24 ans, 25-34 ans, 35-49 ans, 50-64 ans</param>
    /// <returns>Données filtrées par sexe et tranche d'âge</returns>
    /// <response code="200">Retourne les données correspondantes</response>
    /// <response code="404">Aucune donnée trouvée pour ces critères</response>
    [HttpGet("travail-domestique/sexe/{sexe}/tranche/{tranche}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<IEnumerable<TravailDomestique>> GetByGenderAndAge(string sexe, string tranche)
    {
        var result = _context.TravailDomestique
            .Where(t => t.Sexe.ToLower() == sexe.ToLower() && 
                       t.TrancheAge.Contains(tranche, StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        if (!result.Any())
            return NotFound($"Aucune donnée trouvée pour {sexe} dans la tranche {tranche}");
        
        return Ok(result);
    }

    /// <summary>
    /// Récupère les statistiques globales par sexe et activité
    /// </summary>
    /// <returns>Statistiques groupées et filtrées</returns>
    /// <response code="200">Retourne les statistiques</response>
    [HttpGet("travail-domestique/statistiques")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<object> GetStatistiques()
    {
        var stats = _context.TravailDomestique
            .GroupBy(t => new { t.Sexe, t.Activite })
            .Select(g => new
            {
                Sexe = g.Key.Sexe,
                Activite = g.Key.Activite,
                TotteMinutesParJour = g.Sum(x => x.DureeMinutes),
                MoyenneMinutes = g.Average(x => x.DureeMinutes),
                MoyenneCoutJour = g.Average(x => x.CoutJour),
                DetailParTranche = g.GroupBy(x => x.TrancheAge)
                    .Select(ag => new
                    {
                        TrancheAge = ag.Key,
                        Minutes = ag.First().DureeMinutes,
                        CoutJour = ag.First().CoutJour
                    })
                    .OrderBy(x => x.TrancheAge)
                    .ToList()
            })
            .OrderBy(x => x.Sexe)
            .ThenBy(x => x.Activite)
            .ToList();

        return Ok(stats);
    }
}
