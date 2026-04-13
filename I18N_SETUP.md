# Internationalization (i18n) Setup Guide

This app now supports **English**, **French**, and **Spanish**!

## What Was Added

### Frontend (JavaScript)
1. **i18next library** - Industry standard for i18n
   - CDN: `https://cdn.jsdelivr.net/npm/i18next@23.7.6/dist/umd/i18next.min.js`
   - Already included in `index.html`

2. **Translation Files** in `/Frontend/locales/`
   - `en.json` - English translations
   - `fr.json` - French translations  
   - `es.json` - Spanish translations

3. **i18n.js** - Initialization and translation helpers
   - Initializes i18next
   - Auto-detects browser language
   - Provides `t()` and `changeLanguage()` functions
   - Updates UI dynamically

4. **Language Selector** in chat header
   - 🇬🇧 EN, 🇫🇷 FR, 🇪🇸 ES buttons
   - Stored in localStorage (persists on reload)

### Backend (C#)
1. **Language-Specific Questions** in `/Backend/questions/`
   - `en.json` - English questions
   - `fr.json` - French questions
   - `es.json` - Spanish questions

## How It Works

### Frontend
- User selects language via buttons → `changeLanguage(lang)` called
- i18next loads appropriate translation file
- All elements with `data-i18n` attribute updated
- Placeholder text updated via `data-i18n-placeholder`
- Language preference saved in localStorage

### Backend
- App currently loads `questions.json` from `/Backend/`
- Need to modify to load language-specific file based on query parameter

## Next Steps (Code Changes Needed)

### 1. Update BotService to support language
Update `BotService.cs`:
```csharp
public BotService(string questionsFilePath, string language = "en")
{
    // Load language-specific questions
    string langFile = questionsFilePath.Replace(".json", $".{language}.json");
    if (!File.Exists(langFile)) langFile = questionsFilePath; // fallback to en
    LoadQuestions(langFile);
}
```

### 2. Update Controller to accept language parameter
Update `BotController.cs`:
```csharp
private string _currentLanguage = "en";

[HttpGet("language/{language}")]
public IActionResult SetLanguage(string language)
{
    _currentLanguage = language;
    // Reinitialize BotService with new language
    return Ok(new { message = "Language set", language = language });
}
```

### 3. Update questions.json loading in Program.cs
```csharp
// Load default questions (en.json)
services.AddSingleton(new BotService("Backend/questions/en.json"));

// Or make it configurable
var language = config["App:DefaultLanguage"] ?? "en";
services.AddSingleton(new BotService($"Backend/questions/{language}.json"));
```

### 4. Update Frontend to send language to backend
In `chat.js`, when loading questions:
```javascript
const currentLang = i18next.language;
const url = `http://localhost:5000/api/bot/next-question/${currentPersonType}?lang=${currentLang}`;
```

## File Structure
```
Frontend/
  ├── locales/
  │   ├── en.json
  │   ├── fr.json
  │   └── es.json
  ├── i18n.js          (new)
  ├── index.html       (updated)
  ├── chat.js          (ready for updates)
  └── style.css        (updated)

Backend/
  ├── questions/       (new folder)
  │   ├── en.json
  │   ├── fr.json
  │   └── es.json
  └── questions.json   (original, can keep as fallback)
```

## Browser Language Detection
- i18next auto-detects browser language (e.g., `navigator.language = "fr-FR"`)
- Falls back to English if language not supported
- User can override with language buttons
- Choice saved in localStorage

## Translation Keys

All translations are organized hierarchically:
```json
{
  "dashboard": {
    "title": "Dashboard",
    "personal": "Personal Information",
    "expenses": "Expenses"
  },
  "chat": {
    "title": "Chat",
    "placeholder": "Type your message..."
  },
  "buttons": {
    "womanQuestions": "Woman's Questions",
    "manQuestions": "Man's Questions",
    "addExpenses": "Add Expenses"
  }
}
```

## Adding New Translations
1. Add key to all three JSON files in the same location
2. Add `data-i18n="key.path"` to HTML elements
3. Use `t("key.path")` in JavaScript for dynamic text

## Installation (Already Done)
✅ i18next library added via CDN
✅ Locale files created
✅ i18n.js created
✅ HTML updated with i18n attributes
✅ Language selector added
✅ CSS styling added

## What Still Needs Code Changes
- [ ] Update `BotService.cs` to load language-specific questions
- [ ] Update questions.json loading in `Program.cs`
- [ ] Optional: Pass language preference to backend API
- [ ] Test all three languages

## Current Status
- Frontend i18n fully implemented ✅
- Backend questions exist for all languages ✅
- Backend still loads default English questions (needs update)

The app is ready for full i18n implementation with just a few backend changes!
