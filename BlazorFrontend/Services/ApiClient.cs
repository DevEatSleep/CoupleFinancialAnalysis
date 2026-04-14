using System.Net.Http.Json;
using BlazorFrontend.Models;

namespace BlazorFrontend.Services;

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
        return await _http.GetFromJsonAsync<List<Message>>("api/chat") ?? [];
    }

    // Bot questions
    public async Task<BotQuestion?> GetNextQuestionAsync(string? person = null)
    {
        var url = person is not null
            ? $"api/bot/next-question/{person}"
            : "api/bot/next-question";

        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<BotQuestion>();
    }

    public async Task<bool> RespondToBotAsync(BotRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/bot/respond", request);
        return response.IsSuccessStatusCode;
    }

    // Responses (personal info)
    public async Task<List<BotResponseDto>> GetResponsesAsync()
    {
        return await _http.GetFromJsonAsync<List<BotResponseDto>>("api/bot/responses") ?? [];
    }

    public async Task<bool> UpdateResponseAsync(int id, string userResponse)
    {
        var response = await _http.PutAsJsonAsync($"api/bot/response/{id}", new { userResponse });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteResponseAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/bot/response/{id}");
        return response.IsSuccessStatusCode;
    }

    // Expenses
    public async Task<List<Expense>> GetExpensesAsync()
    {
        return await _http.GetFromJsonAsync<List<Expense>>("api/bot/expenses") ?? [];
    }

    public async Task<bool> SaveExpenseAsync(string label, decimal amount, string paidBy)
    {
        var response = await _http.PostAsJsonAsync("api/bot/expense", new { label, amount, paidBy });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateExpenseAsync(int id, Expense expense)
    {
        var response = await _http.PutAsJsonAsync($"api/bot/expense/{id}", expense);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteExpenseAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/bot/expense/{id}");
        return response.IsSuccessStatusCode;
    }

    // Language
    public async Task SetLanguageAsync(string language)
    {
        await _http.PostAsJsonAsync($"api/bot/language/{language}", new { });
    }
}
