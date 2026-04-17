using CoupleChat.Data;
using CoupleChat.Models;
using CoupleChat.Models.Dto;
using CoupleChat.Utilities;
using Microsoft.EntityFrameworkCore;
using Shared;
using SharedConstants = Shared.Constants;

namespace CoupleChat.Services;

/// <summary>
/// Service for managing INSEE reference data on domestic work.
/// Provides a single source of truth for all domestic work reference information.
/// </summary>
public class DomestiqueReferenceService
{
    private readonly ChatDbContext _context;

    public DomestiqueReferenceService(ChatDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all INSEE reference records as DTOs.
    /// </summary>
    public async Task<List<DomestiqueReferenceDto>> GetAllAsync()
    {
        return await _context.TravailDomestique
            .Select(td => new DomestiqueReferenceDto
            {
                Id = td.Id,
                Sexe = td.Sexe,
                Activite = td.Activite,
                TrancheAge = td.TrancheAge,
                DureeMinutes = td.DureeMinutes,
                DureeHeures = td.DureeHeures
            })
            .ToListAsync();
    }

    /// <summary>
    /// Get references filtered by gender (sexe).
    /// </summary>
    public async Task<List<DomestiqueReferenceDto>> GetBySexeAsync(string sexe)
    {
        if (!SharedConstants.Domestique.Sexe.All.Contains(sexe))
            throw new ArgumentException($"Invalid gender: {sexe}");

        return await _context.TravailDomestique
            .Where(td => td.Sexe == sexe)
            .Select(td => new DomestiqueReferenceDto
            {
                Id = td.Id,
                Sexe = td.Sexe,
                Activite = td.Activite,
                TrancheAge = td.TrancheAge,
                DureeMinutes = td.DureeMinutes,
                DureeHeures = td.DureeHeures
            })
            .ToListAsync();
    }

    /// <summary>
    /// Get reference by activity.
    /// </summary>
    public async Task<List<DomestiqueReferenceDto>> GetByActiviteAsync(string activite)
    {
        if (!SharedConstants.Domestique.Activities.All.Contains(activite))
            throw new ArgumentException($"Invalid activity: {activite}");

        return await _context.TravailDomestique
            .Where(td => td.Activite == activite)
            .Select(td => new DomestiqueReferenceDto
            {
                Id = td.Id,
                Sexe = td.Sexe,
                Activite = td.Activite,
                TrancheAge = td.TrancheAge,
                DureeMinutes = td.DureeMinutes,
                DureeHeures = td.DureeHeures
            })
            .ToListAsync();
    }

    /// <summary>
    /// Get reference by gender and age range.
    /// </summary>
    public async Task<List<DomestiqueReferenceDto>> GetBySexeAndAgeRangeAsync(string sexe, string trancheAge)
    {
        if (!SharedConstants.Domestique.Sexe.All.Contains(sexe))
            throw new ArgumentException($"Invalid gender: {sexe}");
        if (!SharedConstants.Domestique.AgeRanges.All.Contains(trancheAge))
            throw new ArgumentException($"Invalid age range: {trancheAge}");

        return await _context.TravailDomestique
            .Where(td => td.Sexe == sexe && td.TrancheAge == trancheAge)
            .Select(td => new DomestiqueReferenceDto
            {
                Id = td.Id,
                Sexe = td.Sexe,
                Activite = td.Activite,
                TrancheAge = td.TrancheAge,
                DureeMinutes = td.DureeMinutes,
                DureeHeures = td.DureeHeures
            })
            .ToListAsync();
    }

    /// <summary>
    /// Get references for a specific activity, returning grouped statistics by gender.
    /// </summary>
    public async Task<List<DomestiqueStatisticsDto>> GetStatisticsByActivityAsync(string activite)
    {
        if (!SharedConstants.Domestique.Activities.All.Contains(activite))
            throw new ArgumentException($"Invalid activity: {activite}");

        return await _context.TravailDomestique
            .Where(td => td.Activite == activite)
            .GroupBy(td => new { td.Sexe, td.Activite })
            .Select(g => new DomestiqueStatisticsDto
            {
                Sexe = g.Key.Sexe,
                Activite = g.Key.Activite,
                AverageMinutesPerDay = (decimal)g.Average(x => x.DureeMinutes),
                AverageHoursPerWeek = (decimal)g.Average(x => x.DureeHeures)
            })
            .ToListAsync();
    }

    /// <summary>
    /// Get the reference value for a specific activity and gender (in hours per week).
    /// Returns null if not found.
    /// </summary>
    public async Task<decimal?> GetActivityReferenceForGenderAsync(string activite, string sexe)
    {
        if (!SharedConstants.Domestique.Activities.All.Contains(activite))
            throw new ArgumentException($"Invalid activity: {activite}");
        if (!SharedConstants.Domestique.Sexe.All.Contains(sexe))
            throw new ArgumentException($"Invalid gender: {sexe}");

        var average = await _context.TravailDomestique
            .Where(td => td.Activite == activite && td.Sexe == sexe)
            .AverageAsync(td => (decimal)td.DureeHeures);

        return average > 0 ? average : null;
    }
}
