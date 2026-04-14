# 💑 Couple Financial Analysis Chat

A multi-language financial management application for couples featuring real-time chat, expense tracking, personal information management, and inline data editing with comprehensive internationalization support.

## 🌟 Features

### Core Features
- ✅ **Real-time Chat** - Instant messaging between partners
- ✅ **Personal Information Dashboard** - Store name, age, and salary for both partners
- ✅ **Expense Tracking** - Log shared, woman-paid, and man-paid expenses
- ✅ **Inline Editing** - Click cells to edit personal info and expenses directly
- ✅ **Delete Functionality** - Remove entries with confirmation dialog
- ✅ **Local Storage** - Messages and user preferences saved in browser

### Internationalization (i18n)
- 🇬🇧 **English**
- 🇫🇷 **French**
- 🇪🇸 **Spanish**
- 🌐 **Auto-detection** - Automatically detects browser language on startup
- 💾 **Persistence** - Saves language preference across sessions
- 🔄 **Complete Translation** - All UI text, messages, and bot questions translated
- 🔌 **Backend Synchronization** - Backend loads language-specific questions on demand

### Backend
- ✅ **Multi-language Questions** - Bot asks questions in selected language
- ✅ **Language API Endpoints** - Switch backend language dynamically
- ✅ **SQLite Database** - Persistent data storage with Entity Framework Core
- ✅ **REST API** - Complete CRUD operations for all data types

## 📁 Project Structure

```
CoupleFinancialAnalysis/
├── Backend/                      # ASP.NET Core 10 REST API
│   ├── Controllers/
│   │   ├── BotController.cs      # Bot Q&A, responses, expenses, language endpoints
│   │   └── ChatController.cs     # Chat messaging
│   ├── Services/
│   │   ├── BotService.cs         # Question management (multi-language support)
│   │   └── NlpProcessor.cs       # Tag/value extraction from responses
│   ├── Models/
│   │   ├── Message.cs
│   │   ├── Response.cs
│   │   ├── Expense.cs
│   │   └── BotRequest.cs
│   ├── Data/
│   │   └── ChatDbContext.cs      # EF Core DbContext
│   ├── questions/                # Language-specific question files
│   │   ├── en.json               # English questions (9 questions)
│   │   ├── fr.json               # French questions (9 questions)
│   │   └── es.json               # Spanish questions (9 questions)
│   ├── questions.json            # Default/fallback questions
│   ├── Program.cs
│   └── CoupleChat.csproj
│
├── Frontend/                     # HTML/CSS/JavaScript PWA
│   ├── index.html               # Main UI with i18n data attributes
│   ├── style.css                # Responsive styling
│   ├── chat.js                  # Chat, Q&A, expense tracking, inline editing
│   ├── dashboard.js             # Dashboard initialization
│   ├── sync.js                  # LocalStorage management
│   ├── i18n.js                  # i18next initialization and language switching
│   ├── locales/                 # Translation files (300+ strings per language)
│   │   ├── en.json              # English translations
│   │   ├── fr.json              # French translations
│   │   └── es.json              # Spanish translations
│   └── test.js                  # Testing utilities
│
├── start.sh                      # Linux startup script
├── start.ps1                     # PowerShell startup script
├── start.bat                     # Windows startup script
├── CoupleFinancialAnalysis.sln
└── README.md
```

## 🚀 Setup & Run

### Prerequisites
- .NET 10 SDK
- Modern browser (Chrome, Firefox, Safari, Edge)

### Quick Start

**Windows:**
```bash
.\start.bat
```

**Linux/Mac:**
```bash
bash start.sh
```

**Manual Setup:**

1. **Backend:**
```bash
cd Backend
dotnet restore
dotnet run
```
Runs on `http://localhost:5000`

2. **Frontend:**
Open browser to `http://localhost:5000`

## 📱 Usage Guide

### Dashboard Layout
- **Left Panel**: Personal information table (name, age, salary) for both partners
- **Right Panel**: Real-time chat conversation
- **Top Right of Dashboard**: Language selector (flag emoji buttons 🇬🇧 🇫🇷 🇪🇸)

### Chat Control Buttons (colored)
- **👩 Woman's Questions** (pink) - Bot asks woman's personal questions
- **👨 Man's Questions** (blue) - Bot asks man's personal questions  
- **💰 Add Expenses** (orange) - Enter expense tracking mode

### Editing Data
1. **Click** any cell in tables (Personal Info or Expenses) to edit
2. **Type** new value directly in the input
3. **Press Enter** to save changes to server
4. **Press Escape** to cancel editing

### Switching Languages
- Click any flag emoji at the top of the dashboard
- Language changes **instantly** for:
  - All UI labels and buttons
  - Chat messages and bot messages
  - Bot questions (backend reloads for selected language)
  - Input placeholders
- Selection is **automatically saved** to localStorage

### Expense Tracking Flow
1. Click **💰 Add Expenses** button
2. Answer **3 questions** for each expense:
   - Expense label (e.g., "Groceries", "Gas")
   - Monthly amount in dollars
   - Who paid: "woman", "man", or "shared"
3. Expense appears in **Expenses table** on dashboard
4. Click **"yes"** to add another expense
5. Click **"done"** to finish

## 🔌 API Endpoints

### Chat
- `GET /api/chat` - Get all messages
- `POST /api/chat` - Send message

### Bot Q&A
- `GET /api/bot/next-question` - Get next general question
- `GET /api/bot/next-question/{person}` - Get next question for person (woman|man|shared)
- `POST /api/bot/respond` - Submit response to question
- `GET /api/bot/responses` - Get all responses
- `PUT /api/bot/response/{id}` - Update response value
- `DELETE /api/bot/response/{id}` - Delete response

### Expenses
- `GET /api/bot/expenses` - Get all expenses
- `POST /api/bot/expense` - Create new expense
- `PUT /api/bot/expense/{id}` - Update expense
- `DELETE /api/bot/expense/{id}` - Delete expense

### Language
- `POST /api/bot/language/{language}` - Set backend language (en|fr|es)
- `GET /api/bot/language` - Get current backend language

## 🗄️ Database Schema

### Responses Table
Stores answers to bot personal questions:
```csharp
{
  "id": int,
  "questionId": int,
  "questionText": string,
  "userResponse": string,
  "category": string,
  "extractedTag": string,
  "person": string,  // "woman" or "man"
  "createdAt": datetime
}
```

### Expenses Table
Stores expense entries:
```csharp
{
  "id": int,
  "label": string,
  "amount": decimal (0.00 - 999999.99),
  "paidBy": string,  // "woman", "man", or "shared"
  "createdAt": datetime
}
```

### Messages Table
Stores chat messages:
```csharp
{
  "id": int,
  "sender": string,
  "content": string,
  "avatarUrl": string,
  "createdAt": datetime
}
```

## 🌐 Internationalization Architecture

### Translation System
- **Library**: i18next v23.7.6 (loaded via CDN)
- **Format**: JSON with hierarchical keys
- **Interpolation**: Supports `{variable}` placeholders
- **Auto-detection**: Browser language detection on startup

### Language Support
| Language | Code | Status |
|----------|------|--------|
| English | en | ✅ Complete |
| French | fr | ✅ Complete |
| Spanish | es | ✅ Complete |

### Translation Files

**Frontend** (`Frontend/locales/{lang}.json`):
- Dashboard labels (title, column headers, tables)
- Chat UI (title, placeholders, buttons)
- Bot messages (welcome, prompts, confirmations)
- System messages (errors, validation)
- Button labels

**Backend** (`Backend/questions/{lang}.json`):
- 3 personal questions for woman
- 3 personal questions for man
- 3 shared expense questions

### Language Detection & Persistence
1. **On App Load**: Detects `navigator.language`
2. **If Supported**: Uses English (en), French (fr), or Spanish (es)
3. **Otherwise**: Falls back to English
4. **User Selection**: Saved to `localStorage.language`
5. **Backend Sync**: Frontend notifies backend of language change

### Persistence Monitoring
- Checks every 1 second if language has been reset
- Auto-restores language preference if needed
- Prevents language resetting during polling updates

## 🎨 Frontend Implementation Details

### chat.js (~800 lines)
**Message Handling:**
- `sendMessage()` - Send chat messages
- `addChatMessage()` - Display messages in chat window
- `loadMessagesFromServer()` - Fetch messages from API

**Bot Interaction:**
- `askWomanQuestions()` / `askManQuestions()` - Start Q&A
- `askNextQuestion()` - Load next question
- `respondToBot()` - Handle user answers
- `loadResponses()` - Display responses in dashboard

**Expense Management:**
- `startExpenseMode()` - Enter expense entry flow
- `saveExpense()` - POST expense to API
- `loadExpenses()` - Fetch and display expenses
- `updateExpense()` - Save edited expense
- `deleteExpense()` - Remove expense with confirmation

**Inline Editing:**
- `handleCellClick()` - Make response cells editable
- `handleExpenseCellClick()` - Make expense cells editable
- `updateResponse()` - Save edited personal info
- Input validation and error handling

**Polling:**
- Runs every 2 seconds
- Checks for new messages, responses, and expenses
- Updates UI without full page reload

### i18n.js (~150 lines)
**Initialization:**
- `initI18n()` - Async load i18next + translation files
- Auto-detect browser language
- Set default language in i18next

**Language Switching:**
- `changeLanguage(lang)` - Switch language globally
- Notifies backend via `POST /api/bot/language/{lang}`
- Updates all UI elements with new translations
- Updates button active state

**UI Updates:**
- `updateUILanguage()` - Update all `[data-i18n]` elements
- `t(key, options)` - Wrapper function for i18next.t()
- Supports variable interpolation

**Persistence:**
- `ensureLanguagePersistence()` - Monitor for unintended resets
- Runs every 1 second
- Auto-restores saved language preference

### style.css (~350 lines)
**Layouts:**
- Flexbox-based 2-column layout (dashboard + chat)
- Responsive design for mobile/tablet
- Fixed language selector positioning

**Components:**
- Chat message bubbles (distinguishes sender)
- Data tables with alternating row colors
- Inline editable cells (yellow highlight on hover)
- Control buttons with individual colors
- Delete button styling (trash icon)
- Language button styling (flag selector)

### sync.js (~100 lines)
**LocalStorage Management:**
- `saveMessage()` - Store message locally
- `getMessages()` - Retrieve messages from storage
- `markMessageSynced()` - Track sent messages

**First Names:**
- `saveFirstName(sender, firstName)` - Store partner names
- `getFirstNames()` - Retrieve saved names
- Used for button labels and chat display

## 🛠️ Tech Stack

### Backend
- **Runtime**: .NET 10
- **Framework**: ASP.NET Core 10
- **Database**: SQLite
- **ORM**: Entity Framework Core
- **Language**: C#

### Frontend
- **Markup**: HTML5
- **Styling**: CSS3 (responsive)
- **Logic**: Vanilla JavaScript ES6+
- **i18n**: i18next v23.7.6 (CDN)
- **Storage**: Browser LocalStorage

### Deployment
- Single ASP.NET Core app serves both API and static files
- Frontend files served from `/Frontend` directory
- CORS enabled for local development

## 📋 Question Categories

### Question Types
- **personal** - Name, age, salary
- **expenses** - Expense tracking (label, amount, payer)
- **financial** - General financial questions (future: budgeting, savings goals)

### Person Types (in questions)
- **woman** - Questions specific to woman
- **man** - Questions specific to man
- **shared** - Expense questions (asked about both)

## 🔄 Data Flow Diagram

```
User Action
    ↓
Frontend (chat.js/i18n.js)
    ↓
REST API (BotController/ChatController)
    ↓
Backend Service (BotService/NlpProcessor)
    ↓
SQLite Database
    ↓
Response to Frontend
    ↓
Update UI (chat + dashboard)
```


## 📄 License

MIT License

## 👥 Contributors

Created for couple financial management and analysis.
