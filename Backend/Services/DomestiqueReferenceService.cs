using CoupleChat.Data;
using CoupleChat.Models;
using CoupleChat.Models.Dto;
using Microsoft.EntityFrameworkCore;
using SharedConstants = Shared.Constants;

namespace CoupleChat.Services;

public class DomestiqueReferenceService
{
    // Single source of truth for default INSEE values — also used by Program.cs seed and the reset endpoint.
    public static readonly (string Activite, string TrancheAge, int FemmeMinutes, int HommeMinutes)[] DefaultInseeData =
    [
        // 18-24 ans
        (SharedConstants.Domestique.Activities.CuisineEtMenage, SharedConstants.Domestique.AgeRanges.Range18_24, 120, 70),
        (SharedConstants.Domestique.Activities.SoinsEnfants,    SharedConstants.Domestique.AgeRanges.Range18_24, 50,  30),
        (SharedConstants.Domestique.Activities.Courses,          SharedConstants.Domestique.AgeRanges.Range18_24, 25,  20),
        (SharedConstants.Domestique.Activities.BricolagJardinage,SharedConstants.Domestique.AgeRanges.Range18_24, 10,  20),
        // 25-34 ans
        (SharedConstants.Domestique.Activities.CuisineEtMenage, SharedConstants.Domestique.AgeRanges.Range25_34, 140, 85),
        (SharedConstants.Domestique.Activities.SoinsEnfants,    SharedConstants.Domestique.AgeRanges.Range25_34, 95,  55),
        (SharedConstants.Domestique.Activities.Courses,          SharedConstants.Domestique.AgeRanges.Range25_34, 32,  28),
        (SharedConstants.Domestique.Activities.BricolagJardinage,SharedConstants.Domestique.AgeRanges.Range25_34, 12,  35),
        // 35-49 ans
        (SharedConstants.Domestique.Activities.CuisineEtMenage, SharedConstants.Domestique.AgeRanges.Range35_49, 150, 90),
        (SharedConstants.Domestique.Activities.SoinsEnfants,    SharedConstants.Domestique.AgeRanges.Range35_49, 105, 60),
        (SharedConstants.Domestique.Activities.Courses,          SharedConstants.Domestique.AgeRanges.Range35_49, 34,  30),
        (SharedConstants.Domestique.Activities.BricolagJardinage,SharedConstants.Domestique.AgeRanges.Range35_49, 15,  40),
        // 50-64 ans
        (SharedConstants.Domestique.Activities.CuisineEtMenage, SharedConstants.Domestique.AgeRanges.Range50_64, 130, 80),
        (SharedConstants.Domestique.Activities.SoinsEnfants,    SharedConstants.Domestique.AgeRanges.Range50_64, 30,  15),
        (SharedConstants.Domestique.Activities.Courses,          SharedConstants.Domestique.AgeRanges.Range50_64, 28,  25),
        (SharedConstants.Domestique.Activities.BricolagJardinage,SharedConstants.Domestique.AgeRanges.Range50_64, 12,  35),
    ];

    private readonly ChatDbContext _context;

    public DomestiqueReferenceService(ChatDbContext context)
    {
        _context = context;
    }

    public async Task<List<DomestiqueReferenceDto>> GetAllAsync()
    {
        return await _context.TravailDomestique
            .Where(td => td.CoupleId == null)
            .Select(td => MapToDto(td))
            .ToListAsync();
    }

    public async Task<List<DomestiqueReferenceDto>> GetBySexeAsync(string sexe)
    {
        if (!SharedConstants.Domestique.Sexe.All.Contains(sexe))
            throw new ArgumentException($"Invalid gender: {sexe}");

        return await _context.TravailDomestique
            .Where(td => td.CoupleId == null && td.Sexe == sexe)
            .Select(td => MapToDto(td))
            .ToListAsync();
    }

    public async Task<List<DomestiqueReferenceDto>> GetByActiviteAsync(string activite)
    {
        if (!SharedConstants.Domestique.Activities.All.Contains(activite))
            throw new ArgumentException($"Invalid activity: {activite}");

        return await _context.TravailDomestique
            .Where(td => td.CoupleId == null && td.Activite == activite)
            .Select(td => MapToDto(td))
            .ToListAsync();
    }

    public async Task<List<DomestiqueReferenceDto>> GetBySexeAndAgeRangeAsync(string sexe, string trancheAge)
    {
        if (!SharedConstants.Domestique.Sexe.All.Contains(sexe))
            throw new ArgumentException($"Invalid gender: {sexe}");
        if (!SharedConstants.Domestique.AgeRanges.All.Contains(trancheAge))
            throw new ArgumentException($"Invalid age range: {trancheAge}");

        return await _context.TravailDomestique
            .Where(td => td.CoupleId == null && td.Sexe == sexe && td.TrancheAge == trancheAge)
            .Select(td => MapToDto(td))
            .ToListAsync();
    }

    public async Task<List<DomestiqueStatisticsDto>> GetStatisticsByActivityAsync(string activite)
    {
        if (!SharedConstants.Domestique.Activities.All.Contains(activite))
            throw new ArgumentException($"Invalid activity: {activite}");

        return await _context.TravailDomestique
            .Where(td => td.CoupleId == null && td.Activite == activite)
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

    public async Task<decimal?> GetActivityReferenceForGenderAsync(string activite, string sexe)
    {
        if (!SharedConstants.Domestique.Activities.All.Contains(activite))
            throw new ArgumentException($"Invalid activity: {activite}");
        if (!SharedConstants.Domestique.Sexe.All.Contains(sexe))
            throw new ArgumentException($"Invalid gender: {sexe}");

        var average = await _context.TravailDomestique
            .Where(td => td.CoupleId == null && td.Activite == activite && td.Sexe == sexe)
            .AverageAsync(td => (decimal)td.DureeHeures);

        return average > 0 ? average : null;
    }

    public async Task<DomestiqueReferenceDto?> UpdateAsync(int id, int dureeMinutes)
    {
        var record = await _context.TravailDomestique.FindAsync(id);
        if (record == null) return null;

        record.DureeMinutes = dureeMinutes;
        record.DureeHeures = Math.Round((decimal)dureeMinutes / 60m, 2);
        record.CoutJour = Math.Round(record.DureeHeures * SharedConstants.Domestique.HourlyRate, 2);

        await _context.SaveChangesAsync();
        return MapToDto(record);
    }

    public async Task<List<DomestiqueReferenceDto>> ResetToDefaultsAsync()
    {
        var existing = await _context.TravailDomestique
            .Where(td => td.CoupleId == null)
            .ToListAsync();
        _context.TravailDomestique.RemoveRange(existing);

        foreach (var (activite, trancheAge, femmeMinutes, hommeMinutes) in DefaultInseeData)
        {
            foreach (var (sexe, minutes) in new[] {
                (SharedConstants.Domestique.Sexe.Femme, femmeMinutes),
                (SharedConstants.Domestique.Sexe.Homme, hommeMinutes)
            })
            {
                var heures = Math.Round((decimal)minutes / 60m, 2);
                _context.TravailDomestique.Add(new TravailDomestique
                {
                    Sexe = sexe,
                    Activite = activite,
                    TrancheAge = trancheAge,
                    DureeMinutes = minutes,
                    DureeHeures = heures,
                    CoutJour = Math.Round(heures * SharedConstants.Domestique.HourlyRate, 2)
                });
            }
        }

        await _context.SaveChangesAsync();
        return await GetAllAsync();
    }

    private static DomestiqueReferenceDto MapToDto(TravailDomestique td) => new()
    {
        Id = td.Id,
        Sexe = td.Sexe,
        Activite = td.Activite,
        TrancheAge = td.TrancheAge,
        DureeMinutes = td.DureeMinutes,
        DureeHeures = td.DureeHeures
    };
}
