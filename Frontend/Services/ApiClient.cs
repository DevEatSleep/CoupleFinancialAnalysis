using System.Net.Http.Json;
using Frontend.Models;
using Shared;
using SharedConstants = Shared.Constants;

namespace Frontend.Services;

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    // Chat
    public async Task<List<Message>> GetMessagesAsync()
    {
        return await _http.GetFromJsonAsync<List<Message>>(SharedConstants.ApiEndpoints.ChatMessages) ?? [];
    }

    // Bot questions
    public async Task<BotQuestion?> GetNextQuestionAsync(string? person = null)
    {
        var url = person is not null
            ? $"{SharedConstants.ApiEndpoints.BotNextQuestion}/{person}"
            : SharedConstants.ApiEndpoints.BotNextQuestion;

        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<BotQuestion>();
    }

    public async Task<bool> RespondToBotAsync(BotRequest request)
    {
        var response = await _http.PostAsJsonAsync(SharedConstants.ApiEndpoints.BotRespond, request);
        return response.IsSuccessStatusCode;
    }

    // Responses (personal info)
    public async Task<List<BotResponseDto>> GetResponsesAsync()
    {
        return await _http.GetFromJsonAsync<List<BotResponseDto>>(SharedConstants.ApiEndpoints.BotResponses) ?? [];
    }

    public async Task<bool> UpdateResponseAsync(int id, string userResponse)
    {
        var response = await _http.PutAsJsonAsync($"{SharedConstants.ApiEndpoints.BotResponse}/{id}", new { userResponse });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteResponseAsync(int id)
    {
        var response = await _http.DeleteAsync($"{SharedConstants.ApiEndpoints.BotResponse}/{id}");
        return response.IsSuccessStatusCode;
    }

    // Expenses
    public async Task<List<Expense>> GetExpensesAsync()
    {
        return await _http.GetFromJsonAsync<List<Expense>>(SharedConstants.ApiEndpoints.BotExpenses) ?? [];
    }

    public async Task<bool> SaveExpenseAsync(string label, decimal amount, string paidBy)
    {
        var response = await _http.PostAsJsonAsync(SharedConstants.ApiEndpoints.BotExpense, new { label, amount, paidBy });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateExpenseAsync(int id, Expense expense)
    {
        var response = await _http.PutAsJsonAsync($"{SharedConstants.ApiEndpoints.BotExpense}/{id}", expense);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteExpenseAsync(int id)
    {
        var response = await _http.DeleteAsync($"{SharedConstants.ApiEndpoints.BotExpense}/{id}");
        return response.IsSuccessStatusCode;
    }

    // Language
    public async Task SetLanguageAsync(string language)
    {
        await _http.PostAsJsonAsync($"{SharedConstants.ApiEndpoints.BotLanguage}/{language}", new { });
    }

    // Travail Domestique
    public async Task<List<DomestiqueResponse>> GetDomestiqueAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<List<DomestiqueResponse>>(SharedConstants.ApiEndpoints.DomestiqueBase) ?? [];
            System.Diagnostics.Debug.WriteLine($"GetDomestique returned {result.Count} records");
            foreach (var item in result)
            {
                System.Diagnostics.Debug.WriteLine($"  - ID:{item.Id} Person:{item.Person} Activity:{item.Activite} Hours:{item.HeuresParSemaine}");
            }
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetDomestique error: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Get all INSEE reference data for domestic work activities.
    /// </summary>
    public async Task<List<DomestiqueReferenceDto>> GetDomestiqueReferencesAsync()
    {
        return await _http.GetFromJsonAsync<List<DomestiqueReferenceDto>>(SharedConstants.ApiEndpoints.TravailDomestique) ?? [];
    }

    /// <summary>
    /// Get reference data for a specific activity (returns average for both genders).
    /// </summary>
    public async Task<List<DomestiqueStatisticsDto>?> GetDomestiqueStatisticsByActivityAsync(string activite)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<DomestiqueStatisticsDto>>($"{SharedConstants.ApiEndpoints.TravailDomestique}/activite/{Uri.EscapeDataString(activite)}");
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Save domestic work hours. The backend will automatically fetch and store INSEE reference values.
    /// </summary>
    public async Task<bool> SaveDomestiqueAsync(string person, string activite, decimal heuresParSemaine)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Saving domestique: person={person}, activite={activite}, heures={heuresParSemaine}");
            
            var payload = new
            {
                person,
                activite,
                heuresParSemaine
            };
            
            var response = await _http.PostAsJsonAsync(SharedConstants.ApiEndpoints.DomestiqueBase, payload);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"SaveDomestique error: {response.StatusCode} - {error}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"SaveDomestique success: {response.StatusCode}");
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SaveDomestique exception: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteDomestiqueAsync(int id)
    {
        var response = await _http.DeleteAsync($"{SharedConstants.ApiEndpoints.DomestiqueBase}/{id}");
        return response.IsSuccessStatusCode;
    }
}
