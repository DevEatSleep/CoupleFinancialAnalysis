using System.Text.Json;
using CoupleChat;
using Shared;
using SharedConstants = Shared.Constants;

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
    private readonly Dictionary<int, int> _womanIndices = new();
    private readonly Dictionary<int, int> _manIndices = new();
    private readonly Dictionary<int, int> _sharedIndices = new();
    private readonly string _questionsBasePath;
    private string _currentLanguage = SharedConstants.Languages.English;

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
        if (SharedConstants.Languages.Supported.Contains(language))
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
        _womanIndices.Clear();
        _manIndices.Clear();
        _sharedIndices.Clear();
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
                var questions = doc.RootElement.GetProperty(SharedConstants.JsonProperties.Questions);
                foreach (var q in questions.EnumerateArray())
                {
                    _questions.Add(new Question
                    {
                        Id = q.GetProperty(SharedConstants.JsonProperties.Id).GetInt32(),
                        Text = q.GetProperty(SharedConstants.JsonProperties.Text).GetString() ?? "",
                        Category = q.GetProperty(SharedConstants.JsonProperties.Category).GetString() ?? "",
                        Person = q.TryGetProperty(SharedConstants.JsonProperties.Person, out var person) ? person.GetString() ?? "" : ""
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

    public Question? GetNextQuestionForPerson(string person, int coupleId)
    {
        // Exclude travail_domestique questions — those are handled entirely by the frontend domestique flow
        var personQuestions = _questions
            .Where(q => q.Person == person && q.Category != SharedConstants.QuestionCategories.TravailDomestique)
            .OrderBy(q => q.Id)
            .ToList();

        if (personQuestions.Count == 0)
            return null;

        // Shared questions (expenses) are a repeating flow: allow cycling so users can add
        // multiple expenses by replying "yes". Personal questions remain single-run.
        if (person == SharedConstants.PersonTypes.Shared)
        {
            var idx = _sharedIndices.GetValueOrDefault(coupleId, 0) % personQuestions.Count;
            var selectedQuestion = personQuestions[idx];
            _sharedIndices[coupleId] = (idx + 1) % personQuestions.Count;
            return selectedQuestion;
        }

        Dictionary<int, int> indices;
        if (person == SharedConstants.PersonTypes.Woman)
            indices = _womanIndices;
        else if (person == SharedConstants.PersonTypes.Man)
            indices = _manIndices;
        else
            return null;

        var index = indices.GetValueOrDefault(coupleId, 0);
        if (index >= personQuestions.Count)
            return null; // All questions for this person have been asked

        var question = personQuestions[index];
        indices[coupleId] = index + 1;
        return question;
    }

    public Question? GetQuestionById(int id)
    {
        return _questions.FirstOrDefault(q => q.Id == id);
    }

    public List<Question> GetAllQuestions() => _questions;
}
