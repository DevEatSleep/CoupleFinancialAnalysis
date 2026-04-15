using System.Net.Http.Json;
using System.Text.Json;

namespace Frontend.Services;

public class LocalizationService
{
    private readonly HttpClient _http;
    private Dictionary<string, Dictionary<string, JsonElement>> _translations = new();
    private string _currentLanguage = "en";

    public event Action? OnLanguageChanged;

    public string CurrentLanguage => _currentLanguage;

    public LocalizationService(HttpClient http)
    {
        _http = http;
    }

    public async Task InitializeAsync()
    {
        var languages = new[] { "en", "fr", "es" };

        foreach (var lang in languages)
        {
            var json = await _http.GetFromJsonAsync<Dictionary<string, JsonElement>>($"locales/{lang}.json");
            if (json is not null)
                _translations[lang] = json;
        }

        // Detect browser language would require JSInterop; default to "en"
        _currentLanguage = "en";
    }

    public void SetLanguage(string language)
    {
        if (_translations.ContainsKey(language))
        {
            _currentLanguage = language;
            OnLanguageChanged?.Invoke();
        }
    }

    public string T(string key)
    {
        if (!_translations.TryGetValue(_currentLanguage, out var langDict))
            return key;

        // Support nested keys like "dashboard.title"
        var parts = key.Split('.');
        if (parts.Length != 2)
            return key;

        if (!langDict.TryGetValue(parts[0], out var section))
            return key;

        if (section.ValueKind != JsonValueKind.Object)
            return key;

        if (section.TryGetProperty(parts[1], out var value))
            return value.GetString() ?? key;

        // Fallback to English
        if (_currentLanguage != "en" && _translations.TryGetValue("en", out var enDict))
        {
            if (enDict.TryGetValue(parts[0], out var enSection) &&
                enSection.ValueKind == JsonValueKind.Object &&
                enSection.TryGetProperty(parts[1], out var enValue))
            {
                return enValue.GetString() ?? key;
            }
        }

        return key;
    }
}
