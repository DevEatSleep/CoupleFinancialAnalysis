using System.Text.Json;

namespace CoupleChat.Services;

public class NlpProcessor
{
    public NlpProcessor(string questionsFilePath)
    {
        // Sentiment analysis removed - no processing needed
    }

    public string ExtractTag(string userResponse, string questionText)
    {
        // No sentiment analysis - return neutral tag
        return "response";
    }
}
