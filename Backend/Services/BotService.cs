using System.Text.Json;

namespace CoupleChat.Services;

public class Question
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Person { get; set; } = string.Empty;
}

public class BotService
{
    private List<Question> _questions = new();
    private int _currentQuestionIndex = 0;
    private int _womanQuestionIndex = 0;
    private int _manQuestionIndex = 0;
    private int _sharedQuestionIndex = 0;
    private string _questionsBasePath;
    private string _currentLanguage = "en";

    public BotService(string questionsFilePath)
    {
        // questionsFilePath should be the base path like "Backend/questions/"
        _questionsBasePath = questionsFilePath.TrimEnd('/').TrimEnd('\\');
        
        // Check if it's a directory. If not, extract the directory
        if (!Directory.Exists(_questionsBasePath))
        {
            _questionsBasePath = Path.GetDirectoryName(questionsFilePath) ?? ".";
        }
        
        LoadQuestions(_currentLanguage);
    }

    public void SetLanguage(string language)
    {
        if (new[] { "en", "fr", "es" }.Contains(language))
        {
            _currentLanguage = language;
            ResetIndices();
            LoadQuestions(language);
        }
    }

    public string GetCurrentLanguage()
    {
        return _currentLanguage;
    }

    private void ResetIndices()
    {
        _currentQuestionIndex = 0;
        _womanQuestionIndex = 0;
        _manQuestionIndex = 0;
        _sharedQuestionIndex = 0;
    }

    private void LoadQuestions(string language)
    {
        try
        {
            // Try to load from language-specific file: questions/{language}.json
            string langFilePath = Path.Combine(_questionsBasePath, $"{language}.json");
            
            // If that doesn't exist, try questions.json in the base path
            if (!File.Exists(langFilePath))
            {
                langFilePath = Path.Combine(Path.GetDirectoryName(_questionsBasePath) ?? ".", "questions.json");
            }

            if (!File.Exists(langFilePath))
            {
                Console.WriteLine($"Warning: Questions file not found at {langFilePath}");
                return;
            }

            _questions.Clear();
            var json = File.ReadAllText(langFilePath);
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                var questions = doc.RootElement.GetProperty("questions");
                foreach (var q in questions.EnumerateArray())
                {
                    _questions.Add(new Question
                    {
                        Id = q.GetProperty("id").GetInt32(),
                        Text = q.GetProperty("text").GetString() ?? "",
                        Category = q.GetProperty("category").GetString() ?? "",
                        Person = q.TryGetProperty("person", out var person) ? person.GetString() ?? "" : ""
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading questions for language '{language}': {ex.Message}");
        }
    }

    public Question? GetNextQuestion()
    {
        if (_questions.Count == 0)
            return null;

        var question = _questions[_currentQuestionIndex];
        _currentQuestionIndex = (_currentQuestionIndex + 1) % _questions.Count;
        return question;
    }

    public Question? GetNextQuestionForPerson(string person)
    {
        var personQuestions = _questions.Where(q => q.Person == person).OrderBy(q => q.Id).ToList();
        
        if (personQuestions.Count == 0)
            return null;

        int index;
        if (person == "woman")
            index = _womanQuestionIndex;
        else if (person == "man")
            index = _manQuestionIndex;
        else if (person == "shared")
            index = _sharedQuestionIndex;
        else
            return null;
        
        if (index >= personQuestions.Count)
            return null; // All questions for this person have been asked

        var question = personQuestions[index];
        
        if (person == "woman")
            _womanQuestionIndex++;
        else if (person == "man")
            _manQuestionIndex++;
        else if (person == "shared")
            _sharedQuestionIndex++;

        return question;
    }

    public Question? GetQuestionById(int id)
    {
        return _questions.FirstOrDefault(q => q.Id == id);
    }

    public List<Question> GetAllQuestions() => _questions;
}
