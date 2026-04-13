using CoupleChat.Data;
using CoupleChat.Models;
using CoupleChat.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoupleChat.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BotController : ControllerBase
{
    private readonly BotService _botService;
    private readonly NlpProcessor _nlpProcessor;
    private readonly ChatDbContext _context;

    public BotController(BotService botService, NlpProcessor nlpProcessor, ChatDbContext context)
    {
        _botService = botService;
        _nlpProcessor = nlpProcessor;
        _context = context;
    }

    [HttpGet("next-question")]
    public ActionResult<dynamic> GetNextQuestion()
    {
        var question = _botService.GetNextQuestion();
        if (question == null)
            return BadRequest("No questions available");

        return Ok(new { id = question.Id, text = question.Text, category = question.Category, person = question.Person });
    }

    [HttpGet("next-question/{person}")]
    public ActionResult<dynamic> GetNextQuestionForPerson(string person)
    {
        var question = _botService.GetNextQuestionForPerson(person);
        if (question == null)
            return BadRequest($"No more questions available for {person}");

        return Ok(new { id = question.Id, text = question.Text, category = question.Category, person = question.Person });
    }

    [HttpPost("respond")]
    public async Task<ActionResult<Response>> PostResponse([FromBody] BotRequest request)
    {
        try
        {
            Console.WriteLine($"[BotController] Received request: QuestionId={request?.QuestionId}, Response={request?.UserResponse}");

            if (request == null || request.QuestionId == 0 || string.IsNullOrEmpty(request.UserResponse))
                return BadRequest("Invalid request");

            var question = _botService.GetQuestionById(request.QuestionId);
            Console.WriteLine($"[BotController] Found question: {question?.Text}");
            
            if (question == null)
                return BadRequest("Question not found");

            string extractedTag = _nlpProcessor.ExtractTag(request.UserResponse, question.Text);
            Console.WriteLine($"[BotController] Extracted tag: {extractedTag}");

            var response = new Response
            {
                QuestionId = request.QuestionId,
                QuestionText = question.Text,
                UserResponse = request.UserResponse,
                Category = question.Category,
                ExtractedTag = extractedTag,
                Person = question.Person,
                CreatedAt = DateTime.UtcNow
            };

            _context.Responses.Add(response);
            await _context.SaveChangesAsync();
            Console.WriteLine($"[BotController] Saved response successfully");

            return Ok(new { 
                id = response.Id, 
                extractedTag = response.ExtractedTag,
                category = response.Category
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BotController] Error in PostResponse: {ex}");
            Console.WriteLine($"[BotController] Inner exception: {ex.InnerException}");
            return StatusCode(500, new { error = ex.Message, details = ex.InnerException?.Message });
        }
    }

    [HttpGet("responses")]
    public ActionResult<IEnumerable<Response>> GetResponses()
    {
        return Ok(_context.Responses.OrderByDescending(r => r.CreatedAt).ToList());
    }

    [HttpPost("expense")]
    public async Task<ActionResult<Expense>> PostExpense([FromBody] Expense expense)
    {
        try
        {
            if (string.IsNullOrEmpty(expense.Label) || expense.Amount <= 0 || string.IsNullOrEmpty(expense.PaidBy))
                return BadRequest("Label, Amount, and PaidBy are required");

            expense.CreatedAt = DateTime.UtcNow;
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return Ok(expense);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BotController] Error in PostExpense: {ex}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("expenses")]
    public ActionResult<IEnumerable<Expense>> GetExpenses()
    {
        return Ok(_context.Expenses.OrderByDescending(e => e.CreatedAt).ToList());
    }

    [HttpPut("expense/{id}")]
    public async Task<IActionResult> UpdateExpense(int id, [FromBody] Expense expense)
    {
        try
        {
            var existingExpense = await _context.Expenses.FindAsync(id);
            if (existingExpense == null)
                return NotFound();

            existingExpense.Label = expense.Label;
            existingExpense.Amount = expense.Amount;
            existingExpense.PaidBy = expense.PaidBy;

            _context.Expenses.Update(existingExpense);
            await _context.SaveChangesAsync();

            return Ok(existingExpense);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("expense/{id}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        try
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
                return NotFound();

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("response/{id}")]
    public async Task<IActionResult> UpdateResponse(int id, [FromBody] Response response)
    {
        try
        {
            var existingResponse = await _context.Responses.FindAsync(id);
            if (existingResponse == null)
                return NotFound();

            existingResponse.UserResponse = response.UserResponse;

            _context.Responses.Update(existingResponse);
            await _context.SaveChangesAsync();

            return Ok(existingResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("response/{id}")]
    public async Task<IActionResult> DeleteResponse(int id)
    {
        try
        {
            var response = await _context.Responses.FindAsync(id);
            if (response == null)
                return NotFound();

            _context.Responses.Remove(response);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("language/{language}")]
    public ActionResult<dynamic> SetLanguage(string language)
    {
        try
        {
            if (!new[] { "en", "fr", "es" }.Contains(language))
                return BadRequest("Invalid language. Supported: en, fr, es");

            _botService.SetLanguage(language);
            return Ok(new { language = _botService.GetCurrentLanguage(), message = "Language updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("language")]
    public ActionResult<dynamic> GetLanguage()
    {
        return Ok(new { language = _botService.GetCurrentLanguage() });
    }
}
