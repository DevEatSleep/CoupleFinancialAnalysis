using Frontend.Models;
using Shared;
using SharedConstants = Shared.Constants;

namespace Frontend.Services;

/// <summary>
/// Service for managing bot interactions including domestic work questions and reference data.
/// Fetches all reference data from the backend API instead of using hardcoded values.
/// </summary>
public class BotService
{
    private readonly ApiClient _api;
    private readonly ChatState _state;
    private readonly LocalizationService _loc;

    // Question ID → (person, activite) mapping
    private static readonly Dictionary<int, (string Person, string Activite)> DomestiqueQuestionMap = new()
    {
        { SharedConstants.QuestionIds.WomanCuisineEtMenage, (SharedConstants.PersonTypes.Woman, SharedConstants.Domestique.Activities.CuisineEtMenage) },
        { SharedConstants.QuestionIds.WomanSoinsEnfants, (SharedConstants.PersonTypes.Woman, SharedConstants.Domestique.Activities.SoinsEnfants) },
        { SharedConstants.QuestionIds.WomanCourses, (SharedConstants.PersonTypes.Woman, SharedConstants.Domestique.Activities.Courses) },
        { SharedConstants.QuestionIds.WomanBricolagJardinage, (SharedConstants.PersonTypes.Woman, SharedConstants.Domestique.Activities.BricolagJardinage) },
        { SharedConstants.QuestionIds.ManCuisineEtMenage, (SharedConstants.PersonTypes.Man, SharedConstants.Domestique.Activities.CuisineEtMenage) },
        { SharedConstants.QuestionIds.ManSoinsEnfants, (SharedConstants.PersonTypes.Man, SharedConstants.Domestique.Activities.SoinsEnfants) },
        { SharedConstants.QuestionIds.ManCourses, (SharedConstants.PersonTypes.Man, SharedConstants.Domestique.Activities.Courses) },
        { SharedConstants.QuestionIds.ManBricolagJardinage, (SharedConstants.PersonTypes.Man, SharedConstants.Domestique.Activities.BricolagJardinage) }
    };

    // Cached INSEE reference data (loaded from the API)
    private Dictionary<string, (decimal InseeRefFemme, decimal InseeRefHomme)> _inseeRefsCache = new();

    // Ordered list of activities for domestique questions
    private static readonly string[] DomestiqueActivites = SharedConstants.Domestique.Activities.All;

    // Index into DomestiqueActivites for the current domestique session
    private int _domestiqueIndex;

    public BotService(ApiClient api, ChatState state, LocalizationService loc)
    {
        _api = api;
        _state = state;
        _loc = loc;
    }

    /// <summary>
    /// Initialize the service by loading reference data from the API.
    /// Call this once on app startup.
    /// </summary>
    public async Task InitializeAsync()
    {
        await LoadInseeReferencesAsync();
    }

    /// <summary>
    /// Fetch INSEE reference data from the backend API and cache locally.
    /// </summary>
    private async Task LoadInseeReferencesAsync()
    {
        try
        {
            var references = await _api.GetDomestiqueReferencesAsync();
            if (references == null || references.Count == 0)
                return;

            _inseeRefsCache.Clear();

            // Group by activity and calculate average hours/week for each gender
            var grouped = references.GroupBy(r => r.Activite);
            foreach (var actGroup in grouped)
            {
                var femmeAvg = (decimal)actGroup
                    .Where(r => r.Sexe == SharedConstants.Domestique.Sexe.Femme)
                    .Average(r => (double)r.DureeHeures);

                var hommeAvg = (decimal)actGroup
                    .Where(r => r.Sexe == SharedConstants.Domestique.Sexe.Homme)
                    .Average(r => (double)r.DureeHeures);

                _inseeRefsCache[actGroup.Key] = (femmeAvg, hommeAvg);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading INSEE references: {ex.Message}");
        }
    }

    public async Task AskQuestionsAsync(string personType)
    {
        _state.ClearChat();
        _state.CurrentPersonType = personType;
        _state.DomestiqueMode = false;
        await AskNextQuestionAsync();
    }

    public async Task AskWomanDomestiqueAsync()
    {
        _state.ClearChat();
        _state.CurrentPersonType = SharedConstants.PersonTypes.Woman;
        _state.DomestiqueMode = true;
        _domestiqueIndex = 0;
        System.Diagnostics.Debug.WriteLine("Starting woman domestique questions");
        AskNextDomestiqueQuestion();
        await Task.CompletedTask;
    }

    public async Task AskManDomestiqueAsync()
    {
        _state.ClearChat();
        _state.CurrentPersonType = SharedConstants.PersonTypes.Man;
        _state.DomestiqueMode = true;
        _domestiqueIndex = 0;
        System.Diagnostics.Debug.WriteLine("Starting man domestique questions");
        AskNextDomestiqueQuestion();
        await Task.CompletedTask;
    }

    private void AskNextDomestiqueQuestion()
    {
        if (_domestiqueIndex >= DomestiqueActivites.Length)
        {
            // All domestique questions answered
            var name = _state.GetFirstName(_state.CurrentPersonType ?? SharedConstants.PersonTypes.Woman);
            var msg = _loc.T("bot.allDone").Replace("{person}", name);
            _state.AddChatMessage("🤖 Bot", msg);
            _state.DomestiqueMode = false;
            _state.CurrentPersonType = null;
            _state.IsBotWaiting = false;
            return;
        }

        var activite = DomestiqueActivites[_domestiqueIndex];
        var person   = _state.CurrentPersonType ?? SharedConstants.PersonTypes.Woman;
        var baseId   = person == SharedConstants.PersonTypes.Woman ? SharedConstants.QuestionIds.WomanDomestiqueBase : SharedConstants.QuestionIds.ManDomestiqueBase;
        
        // Get cached reference values
        var (f, h) = _inseeRefsCache.TryGetValue(activite, out var r) ? r : (0m, 0m);

        var text = _loc.CurrentLanguage switch
        {
            "fr" => $"{activite} – combien d’heures par semaine consacrez-vous à cette tâche\u00a0? (Réf. INSEE\u00a0: femme ~{f}h, homme ~{h}h)",
            "es" => $"{activite} – ¿cuántas horas a la semana? (Ref. INSEE: mujer ~{f}h, hombre ~{h}h)",
            _    => $"{activite} – how many hours per week? (INSEE ref: woman ~{f}h, man ~{h}h)"
        };

        _state.CurrentBotQuestion = new BotQuestion
        {
            Id       = baseId + _domestiqueIndex,
            Text     = text,
            Category = SharedConstants.QuestionCategories.TravailDomestique,
            Person   = person
        };
        _state.IsBotWaiting = true;
        _state.AddChatMessage("🤖 Bot", text);
    }

    public async Task StartExpenseModeAsync()
    {
        _state.ExpenseMode = true;
        _state.CurrentPersonType = null;
        _state.CurrentExpense = new Expense();
        _state.ClearChat();

        _state.AddChatMessage("🤖 Bot", _loc.T("bot.welcome"));

        await Task.Delay(SharedConstants.Timings.ChatMessageDelayMs);
        var question = await _api.GetNextQuestionAsync("shared");
        if (question is not null)
        {
            _state.CurrentBotQuestion = question;
            _state.IsBotWaiting = true;
            _state.AddChatMessage("🤖 Bot", question.Text);
        }
        else
        {
            _state.AddChatMessage("🤖 Bot", _loc.T("bot.error"));
        }
    }

    public async Task AskNextQuestionAsync()
    {
        var person = _state.ExpenseMode ? "shared" : _state.CurrentPersonType;
        var question = await _api.GetNextQuestionAsync(person);

        if (question is not null)
        {
            _state.CurrentBotQuestion = question;
            _state.IsBotWaiting = true;
            _state.AddChatMessage("🤖 Bot", question.Text);
        }
        else
        {
            // All questions answered
            var womanName = _state.GetFirstName("woman");
            var manName = _state.GetFirstName("man");

            if (_state.CurrentPersonType == "woman")
            {
                var msg = _loc.T("bot.allQuestionsAnswered")
                    .Replace("{person}", womanName)
                    .Replace("{otherPerson}", manName);
                _state.AddChatMessage("🤖 Bot", msg);
            }
            else if (_state.CurrentPersonType == SharedConstants.PersonTypes.Man)
            {
                var msg = _loc.T("bot.allDone").Replace("{person}", manName);
                _state.AddChatMessage("🤖 Bot", msg);
            }
            _state.CurrentPersonType = null;
            _state.IsBotWaiting = false;
        }
    }

    public async Task HandleUserInputAsync(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return;

        var lower = content.ToLowerInvariant();
        var yesCmd = _loc.T("commands.yes").ToLowerInvariant();
        var doneCmd = _loc.T("commands.done").ToLowerInvariant();
        var isYes = lower == "yes" || lower == yesCmd;
        var isDone = lower == "done" || lower == doneCmd;

        // Only allow messages when bot is waiting or expense mode
        if (!_state.IsBotWaiting && !_state.ExpenseMode)
            return;

        // Handle expense mode commands (when bot isn't waiting for next answer)
        if (_state.ExpenseMode && !_state.IsBotWaiting)
        {
            if (isDone)
            {
                _state.AddChatMessage(_state.CurrentUser, content);
                _state.AddChatMessage("🤖 Bot", _loc.T("bot.expenseFinished"));
                _state.ExpenseMode = false;
                _state.CurrentExpense = new Expense();
                return;
            }
            else if (isYes)
            {
                _state.AddChatMessage(_state.CurrentUser, content);
                _state.CurrentExpense = new Expense();
                await Task.Delay(300);
                await AskNextQuestionAsync();
                return;
            }
        }

        // If bot is waiting, process the response
        if (_state.IsBotWaiting && _state.CurrentBotQuestion is not null)
        {
            await RespondToBotAsync(content);
        }
    }

    private async Task RespondToBotAsync(string response)
    {
        var question = _state.CurrentBotQuestion!;
        var qId = question.Id;

        // Save first name for woman (Q1) or man (Q4)
        if ((qId == SharedConstants.QuestionIds.WomanFirstName || qId == SharedConstants.QuestionIds.ManFirstName) && _state.CurrentPersonType is not null)
        {
            _state.SaveFirstName(_state.CurrentPersonType, response);
        }

        // Handle expense questions
        if (question.Category == SharedConstants.QuestionCategories.Expenses)
        {
            switch (qId)
            {
                case SharedConstants.QuestionIds.ExpenseLabel:
                    _state.CurrentExpense.Label = response;
                    break;
                case SharedConstants.QuestionIds.ExpenseAmount:
                    if (decimal.TryParse(response, out var amount))
                        _state.CurrentExpense.Amount = amount;
                    break;
                case SharedConstants.QuestionIds.ExpensePaidBy:
                    _state.CurrentExpense.PaidBy = response;

                    // Save the complete expense
                    if (!string.IsNullOrEmpty(_state.CurrentExpense.Label) &&
                        _state.CurrentExpense.Amount > 0 &&
                        !string.IsNullOrEmpty(_state.CurrentExpense.PaidBy))
                    {
                        await _api.SaveExpenseAsync(
                            _state.CurrentExpense.Label,
                            _state.CurrentExpense.Amount,
                            _state.CurrentExpense.PaidBy);

                        var msg = _loc.T("bot.expenseRecorded")
                            .Replace("{label}", _state.CurrentExpense.Label)
                            .Replace("{amount}", _state.CurrentExpense.Amount.ToString())
                            .Replace("{paidBy}", _state.CurrentExpense.PaidBy);

                        _state.AddChatMessage(_state.CurrentUser, response);
                        _state.AddChatMessage("🤖 Bot", msg);
                        _state.CurrentExpense = new Expense();
                        _state.IsBotWaiting = false;
                        _state.CurrentBotQuestion = null;
                        return;
                    }
                    break;
            }

            _state.AddChatMessage(_state.CurrentUser, response);
            _state.IsBotWaiting = false;
            _state.CurrentBotQuestion = null;
            await Task.Delay(SharedConstants.Timings.ChatMessageDelayMs);
            await AskNextQuestionAsync();
            return;
        }

        // Handle travail domestique questions — save to /api/domestique
        // No longer pass hardcoded INSEE ref values; the backend will fetch them
        if (question.Category == SharedConstants.QuestionCategories.TravailDomestique && DomestiqueQuestionMap.TryGetValue(qId, out var mapping))
        {
            if (!decimal.TryParse(response, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var heures))
                heures = 0m;

            // Simply pass person, activity, and hours; the backend handles reference lookup
            try
            {
                var saveSuccess = await _api.SaveDomestiqueAsync(mapping.Person, mapping.Activite, heures);
                if (!saveSuccess)
                {
                    _state.AddChatMessage(_state.CurrentUser, response);
                    _state.AddChatMessage("🤖 Bot", _loc.T("bot.error"));
                    _state.IsBotWaiting = false;
                    _state.CurrentBotQuestion = null;
                    _state.NotifyStateChanged();
                    return;
                }

                _state.AddChatMessage(_state.CurrentUser, response);
                _state.DomestiqueData = await _api.GetDomestiqueAsync();
                _state.IsBotWaiting = false;
                _state.CurrentBotQuestion = null;
                _state.NotifyStateChanged();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving domestique data: {ex.Message}");
                _state.AddChatMessage(_state.CurrentUser, response);
                _state.AddChatMessage("🤖 Bot", _loc.T("bot.error"));
                _state.IsBotWaiting = false;
                _state.CurrentBotQuestion = null;
                _state.NotifyStateChanged();
                return;
            }

            _domestiqueIndex++;
            await Task.Delay(SharedConstants.Timings.ChatMessageDelayMs);
            AskNextDomestiqueQuestion();
            return;
        }

        // Regular personal question
        var success = await _api.RespondToBotAsync(new BotRequest
        {
            QuestionId = question.Id,
            UserResponse = response
        });

        _state.AddChatMessage(_state.CurrentUser, response);
        _state.IsBotWaiting = false;
        _state.CurrentBotQuestion = null;

        if (_state.CurrentPersonType is not null)
        {
            await Task.Delay(SharedConstants.Timings.ChatMessageDelayMs);
            await AskNextQuestionAsync();
        }
    }
}
