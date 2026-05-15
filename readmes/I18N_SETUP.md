# Internationalization (i18n) Implementation

This app fully supports **English**, **French**, and **Spanish** across both backend and frontend!

## Architecture

### Backend (ASP.NET Core)
- Language-specific question files in `/Backend/questions/`
  - `en.json` - English questions
  - `fr.json` - French questions
  - `es.json` - Spanish questions
- `BotService.cs` loads language-specific questions based on current language setting
- Language can be changed via `/api/bot/language/{language}` endpoint

### Frontend (Blazor WebAssembly)
- Translation files in `/Frontend/wwwroot/locales/`
  - `en.json` - English translations
  - `fr.json` - French translations
  - `es.json` - Spanish translations
- `LocalizationService.cs` handles i18n initialization and language switching
- `LanguageSelector.razor` component provides language switching UI (🇬🇧 🇫🇷 🇪🇸)
- Language preference stored in browser localStorage

## How It Works

### Language Selection
1. User clicks language flag in dashboard (🇬🇧 EN, 🇫🇷 FR, 🇪🇸 ES)
2. Frontend calls `LocalizationService.SetLanguage(lang)`
3. Backend receives language change via `/api/bot/language/{language}`
4. All subsequent bot questions are loaded in selected language
5. Language preference saved to localStorage (persistent across sessions)

### Translation System
- Blazor uses `LocalizationService` with i18n.net-style JSON files
- Hierarchical key structure for organized translations
- Components access translations via `@Loc.T("key.path")`
- Support for string interpolation with placeholders

## Supported Languages

| Language | Code | Status |
|----------|------|--------|
| English | en | ✅ Complete |
| French | fr | ✅ Complete |
| Spanish | es | ✅ Complete |

## Translation Coverage

- **Dashboard labels**: Personal info, expenses, household work tables
- **Bot messages**: Greetings, prompts, confirmations
- **Button labels**: All action buttons with language-specific text
- **System messages**: Errors, validations, navigation
- **Field headers**: Table columns and input placeholders
- **Household work (Domestique)**: Activities, monthly hours labels, INSEE references

