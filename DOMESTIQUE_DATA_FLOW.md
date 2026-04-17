# Household Tasks (Travail Domestique) Data Flow Analysis

## Overview
The application successfully implements a complete data flow for household task management, from user input through API endpoints to database storage and dashboard display. This document maps the complete flow and identifies potential issues.

---

## 1. Component Architecture

### DomestiqueTable.razor (Frontend/Components/)
**Purpose**: Display household task data in a dashboard table

**Data Source**: `State.DomestiqueData` (from ChatState service)

**Displays**:
- 4 Activities: "cuisine & ménage", "soins enfants", "courses", "bricolage/jardinage"
- Hours per week (converted from week to month): `HeuresParSemaine × (52/12)`
- INSEE reference values for both genders
- Gap calculation: difference between user hours and INSEE references
- Delete button for removing entries

**Data Structure Expected**:
```csharp
public class DomestiqueResponse
{
    public int Id { get; set; }
    public string Person { get; set; }          // "woman" or "man"
    public string Activite { get; set; }         // Activity name
    public decimal HeuresParSemaine { get; set; } // User-declared hours/week
    public decimal InseeRefFemme { get; set; }    // INSEE ref for women
    public decimal InseeRefHomme { get; set; }    // INSEE ref for men
    public decimal ValeurMonetaire { get; set; }  // Monetary value
    public DateTime CreatedAt { get; set; }
}
```

**Lookup Logic** (L17-24):
- Groups data by activity and person type
- Creates dictionary: `{ activite → { Woman: DomestiqueResponse, Man: DomestiqueResponse } }`
- Renders one row per activity with parallel data for both genders

**Delete Handler** (L93-99):
```csharp
private async Task DeleteRowAsync(int? womanId, int? manId)
{
    if (womanId.HasValue) await Api.DeleteDomestiqueAsync(womanId.Value);
    if (manId.HasValue)   await Api.DeleteDomestiqueAsync(manId.Value);
    State.DomestiqueData = await Api.GetDomestiqueAsync();  // ✅ Refresh after delete
    State.NotifyStateChanged();
}
```

---

## 2. Data Storage: ChatState Service

**Location**: `Frontend/Services/ChatState.cs`

**Key Property**:
```csharp
public List<DomestiqueResponse> DomestiqueData { get; set; } = [];
```

**Scope**: In-memory, lives for the session duration

**Population**: 
- Automatically loaded by `PollingService` every 2 seconds
- Populated from API: `await _api.GetDomestiqueAsync()`
- Updated when:
  - User responds to domestique questions
  - User deletes a record
  - Polling service refreshes dashboard

**State Change Notification**:
- `NotifyStateChanged()` triggers `OnChange` event
- All components subscribed via: `State.OnChange += StateHasChanged`

---

## 3. API Integration: ApiClient Service

**Location**: `Frontend/Services/ApiClient.cs`

### Domestique Endpoints:

#### **GET /api/domestique**
```csharp
public async Task<List<DomestiqueResponse>> GetDomestiqueAsync()
```
- Fetches all user-declared household tasks from backend
- Used by PollingService (every 2 seconds)
- Used after delete operations to refresh UI

#### **POST /api/domestique**
```csharp
public async Task<bool> SaveDomestiqueAsync(string person, string activite, decimal heuresParSemaine)
```
- Saves new household task entry
- Backend automatically:
  - Calculates monetary value: `heures × SMIC hourly rate (11.88€) × week-to-month factor`
  - Fetches INSEE reference values for women and men
  - Stores all values in database

#### **DELETE /api/domestique/{id}**
```csharp
public async Task<bool> DeleteDomestiqueAsync(int id)
```
- Removes a specific domestique response record

#### **GET /api/reference/travail-domestique** (Reference Data)
```csharp
public async Task<List<DomestiqueReferenceDto>> GetDomestiqueReferencesAsync()
```
- Fetches all INSEE reference data (cached by BotService on app startup)
- Used to display comparisons in bot questions and table

---

## 4. User Input → Saving Flow

### Initiation (Home.razor)
User clicks one of two buttons:
```csharp
<button @onclick="Bot.AskWomanDomestiqueAsync">Woman</button>
<button @onclick="Bot.AskManDomestiqueAsync">Man</button>
```

### Question Flow (BotService.cs)

**Step 1: Initialize Session** (L94-103)
```csharp
public async Task AskWomanDomestiqueAsync()
{
    _state.ClearChat();
    _state.CurrentPersonType = "woman";      // Set person type
    _state.DomestiqueMode = true;             // Enter domestique mode
    _domestiqueIndex = 0;                     // Start at activity 0
    AskNextDomestiqueQuestion();
}
```

**Step 2: Ask Questions in Sequence** (L105-131)
```csharp
private void AskNextDomestiqueQuestion()
{
    if (_domestiqueIndex >= DomestiqueActivites.Length) { /* all done */ }
    
    var activite = DomestiqueActivites[_domestiqueIndex];  // cuisine & ménage, etc.
    var person = _state.CurrentPersonType;                 // woman or man
    var (f, h) = _inseeRefsCache.TryGetValue(activite, ...); // INSEE values
    
    // Create question with INSEE references displayed
    var text = $"{activite} – how many hours per week? (INSEE ref: woman ~{f}h, man ~{h}h)";
    
    _state.CurrentBotQuestion = new BotQuestion
    {
        Id = baseId + _domestiqueIndex,
        Text = text,
        Category = "travail_domestique",
        Person = person
    };
    _state.IsBotWaiting = true;
    _state.AddChatMessage("🤖 Bot", text);
}
```

**Step 3: User Responds** (Home.razor)
```csharp
private async Task SendMessage()
{
    var text = InputText;
    InputText = "";
    await Bot.HandleUserInputAsync(text);
}
```

**Step 4: Handle Response** (BotService.cs L322-337)
```csharp
// Handle travail domestique questions
if (question.Category == "travail_domestique" && 
    DomestiqueQuestionMap.TryGetValue(qId, out var mapping))
{
    // Parse user input as decimal hours
    if (!decimal.TryParse(response, ..., out var heures))
        heures = 0m;

    // 🔑 SAVE TO BACKEND - backend handles INSEE lookup
    await _api.SaveDomestiqueAsync(mapping.Person, mapping.Activite, heures);

    _state.AddChatMessage(_state.CurrentUser, response);
    _state.DomestiqueData = await _api.GetDomestiqueAsync();  // ✅ Refresh
    _state.IsBotWaiting = false;
    _state.CurrentBotQuestion = null;
    _state.NotifyStateChanged();

    _domestiqueIndex++;  // Move to next activity
    await Task.Delay(300);
    AskNextDomestiqueQuestion();  // Continue questioning
    return;
}
```

**Step 5: Backend Processing** (Backend/Controllers/DomestiqueController.cs L37-76)
```csharp
[HttpPost]
public async Task<ActionResult<DomestiqueResponseDto>> Create([FromBody] DomestiqueResponse response)
{
    // Validate input
    if (string.IsNullOrWhiteSpace(response.Person) || 
        string.IsNullOrWhiteSpace(response.Activite))
        return BadRequest("Person and Activite are required.");

    // Set timestamp and calculate monetary value
    response.CreatedAt = DateTime.UtcNow;
    response.ValeurMonetaire = response.HeuresParSemaine * 
                               WeekToMonthFactor * 
                               HourlyRate;  // 11.88€/hour
    
    // 🔑 FETCH INSEE REFERENCES
    var referenceFemme = await _referenceService
        .GetActivityReferenceForGenderAsync(response.Activite, "femme");
    var referenceHomme = await _referenceService
        .GetActivityReferenceForGenderAsync(response.Activite, "homme");

    response.InseeRefFemme = (decimal)(referenceFemme ?? 0);
    response.InseeRefHomme = (decimal)(referenceHomme ?? 0);

    // Save to database
    _context.DomestiqueResponses.Add(response);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetAll), new { id = response.Id }, responseDto);
}
```

**Step 6: Database Storage** (Backend/Data/ChatDbContext.cs)
```
Table: DomestiqueResponses
├── Id
├── Person ("woman" | "man")
├── Activite ("cuisine & ménage" | "soins enfants" | "courses" | "bricolage/jardinage")
├── HeuresParSemaine (user input)
├── InseeRefFemme (fetched from TravailDomestique table)
├── InseeRefHomme (fetched from TravailDomestique table)
├── ValeurMonetaire (calculated: hours × SMIC × 52/12)
└── CreatedAt (UTC timestamp)
```

---

## 5. Data Refresh → Dashboard Display

### Automatic Polling (PollingService)

**Location**: `Frontend/Services/PollingService.cs`

**Initialization** (Home.razor L176):
```csharp
protected override void OnInitialized()
{
    Polling.Start();  // Start polling on page load
}
```

**Polling Loop**:
```csharp
public void Start()
{
    _timer = new PeriodicTimer(TimeSpan.FromSeconds(2));  // Poll every 2 seconds
    _ = PollLoopAsync(_cts.Token);
}

private async Task PollLoopAsync(CancellationToken ct)
{
    // Initial load
    await LoadDashboardDataAsync();
    _state.IsConnected = true;

    while (await _timer!.WaitForNextTickAsync(ct))
    {
        try
        {
            await LoadDashboardDataAsync();
            _state.IsConnected = true;
        }
        catch
        {
            _state.IsConnected = false;
        }
        _state.NotifyStateChanged();  // 🔑 Trigger UI refresh
    }
}

private async Task LoadDashboardDataAsync()
{
    // ... load responses ...
    _state.WomanData = woman;
    _state.ManData = man;

    // ... load expenses ...
    _state.Expenses = await _api.GetExpensesAsync();

    // 🔑 LOAD DOMESTIQUE DATA
    _state.DomestiqueData = await _api.GetDomestiqueAsync();
}
```

### Display Update Cycle

```
Polling Service (every 2 seconds)
    ↓
GetDomestiqueAsync() → API → Backend
    ↓
Controller.GetAll() → ChatDbContext.DomestiqueResponses.ToList()
    ↓
Response → ChatState.DomestiqueData
    ↓
NotifyStateChanged() → State.OnChange event
    ↓
DomestiqueTable.razor subscribes to State.OnChange
    ↓
StateHasChanged() triggered
    ↓
Razor component re-renders with new data
```

---

## 6. Complete Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│  USER INTERACTION PHASE                                         │
└─────────────────────────────────────────────────────────────────┘

Home.razor
    ↓ User clicks "Ask Woman Domestique"
BotService.AskWomanDomestiqueAsync()
    ↓ Sets: CurrentPersonType="woman", DomestiqueMode=true
AskNextDomestiqueQuestion() [activity 0]
    ↓ Displays: "cuisine & ménage – how many hours/week?"
    ↓ Gets INSEE references from BotService cache

User Types Response (e.g., "10")
    ↓
Home.razor.SendMessage()
    ↓
BotService.HandleUserInputAsync("10")
    ↓
BotService.RespondToBotAsync("10")
    ↓
Bot checks: question.Category == "travail_domestique"
    ↓
Parse "10" → heures = 10m

┌─────────────────────────────────────────────────────────────────┐
│  API/BACKEND PHASE                                              │
└─────────────────────────────────────────────────────────────────┘

ApiClient.SaveDomestiqueAsync("woman", "cuisine & ménage", 10)
    ↓
HTTP POST /api/domestique
    {
      "person": "woman",
      "activite": "cuisine & ménage",
      "heuresParSemaine": 10
    }
    ↓
DomestiqueController.Create()
    ↓ Validate input
    ↓ Calculate ValeurMonetaire = 10 × (52/12) × 11.88 ≈ 51.56€
    ↓
DomestiqueReferenceService.GetActivityReferenceForGenderAsync()
    ↓ Query TravailDomestique table
    ↓ Average hours for "femme" + "cuisine & ménage"
    ↓ Returns: 2.5 hours/week (example)
    ↓
DomestiqueResponse object populated:
    {
      Id: [auto-generated],
      Person: "woman",
      Activite: "cuisine & ménage",
      HeuresParSemaine: 10,
      InseeRefFemme: 2.5,    ← FROM DATABASE
      InseeRefHomme: 1.2,    ← FROM DATABASE
      ValeurMonetaire: 51.56,
      CreatedAt: [now]
    }
    ↓
Save to ChatDbContext.DomestiqueResponses
    ↓
HTTP 201 Created response

┌─────────────────────────────────────────────────────────────────┐
│  UI REFRESH PHASE                                               │
└─────────────────────────────────────────────────────────────────┘

BotService immediately reloads:
    ↓
ApiClient.GetDomestiqueAsync()
    ↓
HTTP GET /api/domestique
    ↓
DomestiqueController.GetAll()
    ↓
return _context.DomestiqueResponses.ToList()
    ↓
Response with ALL domestique entries (including new one)
    ↓
ChatState.DomestiqueData = [... all entries ...]
    ↓
NotifyStateChanged()
    ↓
All subscribers re-render (DomestiqueTable.razor)

Continue to next activity:
    ↓
AskNextDomestiqueQuestion() [activity 1]
    ↓ "soins enfants – how many hours/week?"
    [cycle repeats]

┌─────────────────────────────────────────────────────────────────┐
│  DASHBOARD POLLING PHASE (CONTINUOUS)                           │
└─────────────────────────────────────────────────────────────────┘

PollingService (every 2 seconds):
    ↓
LoadDashboardDataAsync()
    ↓
GetDomestiqueAsync() from API
    ↓
ChatState.DomestiqueData = [refreshed list]
    ↓
NotifyStateChanged()
    ↓
DomestiqueTable.razor re-renders
    ↓
Lookup data and display in table
```

---

## 7. Potential Issues & Solutions

### ⚠️ ISSUE #1: Race Condition in BotService Response Handler
**Location**: BotService.cs L330

**Problem**:
```csharp
await _api.SaveDomestiqueAsync(mapping.Person, mapping.Activite, heures);
_state.DomestiqueData = await _api.GetDomestiqueAsync();
```
- Both calls happen sequentially
- If POST is slow, GET might happen before DB write completes
- Data might not be reflected immediately

**Impact**: Minimal (polling will catch it in ≤2s)

**Solution**:
```csharp
// Add small delay to ensure DB write completes
await _api.SaveDomestiqueAsync(mapping.Person, mapping.Activite, heures);
await Task.Delay(100);  // Brief wait for DB write
_state.DomestiqueData = await _api.GetDomestiqueAsync();
```

---

### ⚠️ ISSUE #2: Question Validation Not Checking Activity Value
**Location**: BotService.cs L320

**Problem**:
```csharp
var mapping = DomestiqueQuestionMap[qId];  // What if qId is invalid?
```
- If user answers with invalid question ID, parsing fails silently
- No validation that `mapping.Activite` is in `SharedConstants.Domestique.Activities.All`

**Impact**: Could create orphaned entries if questions are modified

**Solution**: Add explicit validation
```csharp
if (!DomestiqueQuestionMap.TryGetValue(qId, out var mapping))
{
    _state.AddChatMessage("🤖 Bot", "Invalid question format");
    return;
}

if (!SharedConstants.Domestique.Activities.All.Contains(mapping.Activite))
{
    _state.AddChatMessage("🤖 Bot", "Invalid activity");
    return;
}
```

---

### ⚠️ ISSUE #3: No Validation on Hours Input
**Location**: BotService.cs L324-325

**Problem**:
```csharp
if (!decimal.TryParse(response, ..., out var heures))
    heures = 0m;  // Silent failure! User won't know input was invalid
```

**Impact**: User types "abc hours" → backend saves 0 hours with no feedback

**Solution**:
```csharp
if (!decimal.TryParse(response, System.Globalization.NumberStyles.Any,
    System.Globalization.CultureInfo.InvariantCulture, out var heures))
{
    _state.AddChatMessage("🤖 Bot", _loc.T("bot.invalidHours"));
    _state.IsBotWaiting = true;  // Ask again
    return;
}

if (heures < 0)
{
    _state.AddChatMessage("🤖 Bot", _loc.T("bot.negativeHours"));
    _state.IsBotWaiting = true;
    return;
}
```

---

### ⚠️ ISSUE #4: INSEE References Not Loaded Before Domestique Questions
**Location**: Frontend/Program.cs L44-46

**Current Implementation**:
```csharp
var botService = host.Services.GetRequiredService<BotService>();
await botService.InitializeAsync();  // ✅ Loads INSEE data
```

**Status**: ✅ **CORRECT** - References are pre-loaded on app startup

**Verification**:
```csharp
// BotService.InitializeAsync() (L53-65)
private async Task LoadInseeReferencesAsync()
{
    var references = await _api.GetDomestiqueReferencesAsync();
    // ... group by activity and cache locally ...
    _inseeRefsCache[actGroup.Key] = (femmeAvg, hommeAvg);
}

// Later, when asking questions:
var (f, h) = _inseeRefsCache.TryGetValue(activite, out var r) ? r : (0m, 0m);
```

---

### ⚠️ ISSUE #5: DomestiqueTable Renders Before Data Loads
**Location**: DomestiqueTable.razor L29-31

**Problem**:
```csharp
@if (!hasData)
{
    <tr><td colspan="7" class="empty-state">@Loc.T("dashboard.noDomestique")</td></tr>
}
```

**Potential Issue**: 
- If polling hasn't completed yet when page loads, shows "No data"
- Data appears after first polling cycle (≤2 seconds)
- **This is acceptable UX** but could be confusing

**Workaround**: Show loading state
```csharp
<div class="empty-state">
    @if (_isLoading)
    {
        <p>@Loc.T("common.loading")</p>
    }
    else
    {
        <p>@Loc.T("dashboard.noDomestique")</p>
    }
</div>
```

---

### ⚠️ ISSUE #6: No Error Handling on API Failures
**Location**: Multiple locations

**Examples**:
- BotService.cs L330: `await _api.SaveDomestiqueAsync(...)` - no error check
- DomestiqueTable.razor L99: `await Api.DeleteDomestiqueAsync(id)` - no error check

**Impact**: 
- Silent failures if backend is down
- User might think data was saved when it wasn't

**Solution**:
```csharp
var success = await _api.SaveDomestiqueAsync(mapping.Person, mapping.Activite, heures);
if (!success)
{
    _state.AddChatMessage("🤖 Bot", _loc.T("bot.saveFailed"));
    _state.IsBotWaiting = true;  // Let user retry
    return;
}
```

---

## 8. Data Flow Verification Checklist

### Frontend → API
- ✅ ApiClient has methods for all domestique operations (GET, POST, DELETE)
- ✅ Methods use SharedConstants.ApiEndpoints for URLs
- ✅ SaveDomestiqueAsync passes person, activite, heuresParSemaine
- ⚠️ **Missing**: Error handling and success/failure feedback

### API → Backend
- ✅ POST endpoint validates person and activite
- ✅ Calculates ValeurMonetaire correctly
- ✅ Fetches INSEE references automatically
- ✅ Stores all data in DomestiqueResponses table

### Backend → Frontend
- ✅ GET endpoint returns complete DomestiqueResponse objects
- ✅ Includes INSEE references (InseeRefFemme, InseeRefHomme)
- ✅ PollingService polls every 2 seconds
- ⚠️ **Missing**: Configurable polling interval, connection status details

### Frontend Display
- ✅ DomestiqueTable correctly displays all activities
- ✅ Converts hours/week to hours/month with correct formula
- ✅ Shows INSEE references in gray text
- ✅ Calculates and displays gap with color coding
- ✅ Delete button refreshes data
- ⚠️ **Missing**: Loading states, empty data states

---

## 9. Quick Reference: Adding a New Household Activity

To add a new activity (e.g., "shopping"):

### 1. Update Constants (Shared/Constants.cs)
```csharp
public static class Activities
{
    public const string Shopping = "shopping";  // Add this
    
    public static readonly string[] All = 
    {
        CuisineEtMenage,
        SoinsEnfants,
        Courses,
        BricolagJardinage,
        Shopping  // Add to array
    };
}
```

### 2. Update QuestionIds (if needed)
```csharp
public static class QuestionIds
{
    // Add new question IDs...
    public const int WomanShopping = 21;
    public const int ManShopping = 22;
}
```

### 3. Update BotService (Frontend/Services/BotService.cs)
```csharp
private static readonly Dictionary<int, (string Person, string Activite)> DomestiqueQuestionMap = new()
{
    // ... existing entries ...
    { SharedConstants.QuestionIds.WomanShopping, ("woman", "shopping") },
    { SharedConstants.QuestionIds.ManShopping, ("man", "shopping") }
};
```

### 4. Add INSEE Reference Data (Backend/Program.cs)
```csharp
var donneesInsee = new (string, string, int, int)[]
{
    // ... existing ...
    ("shopping", "18-24 ans", 15, 12),
    ("shopping", "25-34 ans", 20, 15),
    // ... add for all age ranges ...
};
```

### 5. Update DomestiqueTable.razor
```csharp
var activites = new[] 
{ 
    "cuisine & ménage", 
    "soins enfants", 
    "courses", 
    "bricolage/jardinage",
    "shopping"  // Add this
};
```

---

## 10. Summary

The household tasks data flow is **well-architected and functional**:

| Component | Status | Notes |
|-----------|--------|-------|
| User Input Flow | ✅ Working | Seamless chat-based input |
| Question Management | ✅ Working | Sequential, with INSEE refs displayed |
| Data Validation | ⚠️ Minimal | Missing input range validation |
| Backend Processing | ✅ Excellent | Calculates values, fetches references |
| Database Storage | ✅ Correct | All required fields stored |
| Data Retrieval | ✅ Working | GET endpoint returns complete data |
| Polling & Refresh | ✅ Working | 2-second interval ensures fresh data |
| Dashboard Display | ✅ Working | Table renders correctly with all data |
| Error Handling | ❌ Missing | No user feedback on failures |
| Loading States | ⚠️ Minimal | Shows "No data" initially, then updates |

**Main Recommendation**: Add error handling and user feedback for API failures, especially in the save/delete operations.
