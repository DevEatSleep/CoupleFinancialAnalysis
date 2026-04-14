using BlazorFrontend.Models;

namespace BlazorFrontend.Services;

public class BotService
{
    private readonly ApiClient _api;
    private readonly ChatState _state;
    private readonly LocalizationService _loc;

    public BotService(ApiClient api, ChatState state, LocalizationService loc)
    {
        _api = api;
        _state = state;
        _loc = loc;
    }

    public async Task AskQuestionsAsync(string personType)
    {
        _state.ClearChat();
        _state.CurrentPersonType = personType;
        await AskNextQuestionAsync();
    }

    public async Task StartExpenseModeAsync()
    {
        _state.ExpenseMode = true;
        _state.CurrentPersonType = null;
        _state.CurrentExpense = new Expense();
        _state.ClearChat();

        _state.AddChatMessage("🤖 Bot", _loc.T("bot.welcome"));

        await Task.Delay(300);
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
            else if (_state.CurrentPersonType == "man")
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
        if (qId is 1 or 4 && _state.CurrentPersonType is not null)
        {
            _state.SaveFirstName(_state.CurrentPersonType, response);
        }

        // Handle expense questions
        if (question.Category == "expenses")
        {
            switch (qId)
            {
                case 7:
                    _state.CurrentExpense.Label = response;
                    break;
                case 8:
                    if (decimal.TryParse(response, out var amount))
                        _state.CurrentExpense.Amount = amount;
                    break;
                case 9:
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
            await Task.Delay(300);
            await AskNextQuestionAsync();
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
            await Task.Delay(300);
            await AskNextQuestionAsync();
        }
    }
}
