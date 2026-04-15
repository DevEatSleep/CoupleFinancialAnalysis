using Frontend.Models;

namespace Frontend.Services;

public class ChatState
{
    public string CurrentUser { get; set; } = "Person1";
    public BotQuestion? CurrentBotQuestion { get; set; }
    public bool IsBotWaiting { get; set; }
    public string? CurrentPersonType { get; set; } // "woman" or "man"
    public bool ExpenseMode { get; set; }
    public bool DomestiqueMode { get; set; }       // true when in travail domestique session
    public Expense CurrentExpense { get; set; } = new();

    // First names stored in memory (replaces localStorage)
    public Dictionary<string, string> FirstNames { get; set; } = new();

    // Chat messages displayed in UI
    public List<ChatMessage> Messages { get; set; } = [];

    // Dashboard data
    public PersonData WomanData { get; set; } = new();
    public PersonData ManData { get; set; } = new();
    public List<Expense> Expenses { get; set; } = [];
    public List<DomestiqueResponse> DomestiqueData { get; set; } = [];

    // Connection status
    public bool IsConnected { get; set; }

    public event Action? OnChange;

    public void NotifyStateChanged() => OnChange?.Invoke();

    public void SaveFirstName(string person, string name)
    {
        FirstNames[person] = name;
        NotifyStateChanged();
    }

    public string GetFirstName(string person)
    {
        return FirstNames.TryGetValue(person, out var name) ? name : person switch
        {
            "woman" => "Woman",
            "man" => "Man",
            _ => person
        };
    }

    public string GetDisplayName(string sender)
    {
        if (sender == "🤖 Bot") return "🤖 Bot";
        if (sender == "Person1") return GetFirstName("woman");
        if (sender == "Person2") return GetFirstName("man");
        return sender;
    }

    public void AddChatMessage(string sender, string content)
    {
        Messages.Add(new ChatMessage
        {
            Sender = sender,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            IsMine = sender == CurrentUser
        });
        NotifyStateChanged();
    }

    public void ClearChat()
    {
        Messages.Clear();
        NotifyStateChanged();
    }
}

public class ChatMessage
{
    public string Sender { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsMine { get; set; }
}
