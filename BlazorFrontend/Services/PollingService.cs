using BlazorFrontend.Models;

namespace BlazorFrontend.Services;

public class PollingService : IAsyncDisposable
{
    private readonly ApiClient _api;
    private readonly ChatState _state;
    private PeriodicTimer? _timer;
    private CancellationTokenSource? _cts;

    public PollingService(ApiClient api, ChatState state)
    {
        _api = api;
        _state = state;
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(2));
        _ = PollLoopAsync(_cts.Token);
    }

    private async Task PollLoopAsync(CancellationToken ct)
    {
        // Initial load
        await LoadDashboardDataAsync();
        _state.IsConnected = true;
        _state.NotifyStateChanged();

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
            _state.NotifyStateChanged();
        }
    }

    private async Task LoadDashboardDataAsync()
    {
        // Load responses and organize into PersonData
        var responses = await _api.GetResponsesAsync();
        var woman = new PersonData();
        var man = new PersonData();

        foreach (var resp in responses)
        {
            var target = resp.Person == "woman" ? woman : resp.Person == "man" ? man : null;
            if (target is null) continue;

            if (resp.Category == "personal")
            {
                if (resp.QuestionId is 1 or 4) { target.Name = resp.UserResponse; target.NameId = resp.Id; }
                else if (resp.QuestionId is 2 or 5) { target.Age = resp.UserResponse; target.AgeId = resp.Id; }
            }
            else if (resp.Category == "financial")
            {
                if (resp.QuestionId is 3 or 6) { target.Salary = resp.UserResponse; target.SalaryId = resp.Id; }
            }
        }

        _state.WomanData = woman;
        _state.ManData = man;

        // Load expenses
        _state.Expenses = await _api.GetExpensesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        _cts?.Cancel();
        _timer?.Dispose();
        _cts?.Dispose();
    }
}
