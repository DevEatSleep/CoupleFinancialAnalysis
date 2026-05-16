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
            var response = await _http.PostAsJsonAsync(Constants.ApiEndpoints.AuthRegister, request);
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
            var response = await _http.PostAsJsonAsync(Constants.ApiEndpoints.AuthLogin, request);
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
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _http.PostAsJsonAsync(Constants.ApiEndpoints.AuthVerify, new { });
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<AuthResponse>();
            return null;
        }
        catch
        {
            return null;
        }
    }

    public event Action? OnAuthStateChanged;

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
        OnAuthStateChanged?.Invoke();
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

            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _currentToken);
            var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, Constants.ApiEndpoints.AuthDeleteAccount)
            {
                Content = JsonContent.Create(new DeleteAccountRequest { Password = password })
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

    public async Task<(bool Success, string Error)> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        try
        {
            if (string.IsNullOrEmpty(_currentToken))
                return (false, "Non connecté");

            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _currentToken);
            var request = new ChangePasswordRequest
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };
            var response = await _http.PostAsJsonAsync(Constants.ApiEndpoints.AuthChangePassword, request);
            if (response.IsSuccessStatusCode)
                return (true, "");

            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public void Logout()
    {
        _currentToken = null;
        _currentUser = null;
        OnAuthStateChanged?.Invoke();
    }
}

