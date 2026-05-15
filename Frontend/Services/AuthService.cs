using System.Net.Http.Json;
using Frontend.Models;
using Shared;

namespace Frontend.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private AuthUser? _currentUser;
    private string? _currentToken;

    public AuthService(HttpClient http)
    {
        _http = http;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"{Constants.Network.ServerUrl}/api/auth/register", request);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<AuthResponse>();
            
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Register failed: {response.StatusCode} - {error}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Register error: {ex.Message}");
            return null;
        }
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"{Constants.Network.ServerUrl}/api/auth/login", request);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<AuthResponse>();
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<AuthResponse?> VerifyTokenAsync(string token)
    {
        try
        {
            var request = new { token };
            var response = await _http.PostAsJsonAsync($"{Constants.Network.ServerUrl}/api/auth/verify", request);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<AuthResponse>();
            return null;
        }
        catch
        {
            return null;
        }
    }

    public void SaveToken(AuthResponse authResponse)
    {
        if (authResponse == null)
            return;

        _currentToken = authResponse.Token;
        _currentUser = new AuthUser
        {
            UserId = authResponse.UserId,
            CoupleId = authResponse.CoupleId,
            Email = authResponse.Email,
            Name = authResponse.Name,
            Token = authResponse.Token
        };
    }

    public AuthUser? GetCurrentUser()
    {
        return _currentUser;
    }

    public string? GetToken()
    {
        return _currentToken;
    }

    public bool IsAuthenticated()
    {
        return !string.IsNullOrEmpty(_currentToken);
    }

    public async Task<bool> DeleteAccountAsync(string password)
    {
        try
        {
            if (string.IsNullOrEmpty(_currentToken))
                return false;

            var request = new { token = _currentToken, password };
            var response = await _http.DeleteAsync($"{Constants.Network.ServerUrl}/api/auth/delete-account");
            
            // For DELETE with body, we need to use a custom method
            var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"{Constants.Network.ServerUrl}/api/auth/delete-account")
            {
                Content = JsonContent.Create(request)
            };
            
            var deleteResponse = await _http.SendAsync(deleteRequest);
            return deleteResponse.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Delete account error: {ex.Message}");
            return false;
        }
    }

    public void Logout()
    {
        _currentToken = null;
        _currentUser = null;
    }
}

