using CoupleChat.Data;
using CoupleChat.Models;
using CoupleChat.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared;
using SharedConstants = Shared.Constants;

namespace CoupleChat.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SurveyController : ControllerBase
{
    private readonly BotService _botService;
    private readonly ChatDbContext _context;

    public SurveyController(BotService botService, ChatDbContext context)
    {
        _botService = botService;
        _context = context;
    }

    private int GetCoupleId()
    {
        var coupleIdObj = HttpContext.Items["CoupleId"];
        if (coupleIdObj is int coupleId)
            return coupleId;

        throw new UnauthorizedAccessException("CoupleId not found in token");
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
        var coupleId = GetCoupleId();
        var answered = _context.Responses
            .Where(r => r.CoupleId == coupleId)
            .Select(r => r.QuestionId)
            .ToHashSet();
        var question = _botService.GetNextQuestionForPerson(person, answered);
        if (question == null)
            return BadRequest($"No more questions available for {person}");

        return Ok(new { id = question.Id, text = question.Text, category = question.Category, person = question.Person });
    }

    [HttpPost("responses")]
    public async Task<ActionResult<Response>> PostResponse([FromBody] BotRequest request)
    {
        try
        {
            var coupleId = GetCoupleId();

            if (request == null || request.QuestionId == 0 || string.IsNullOrEmpty(request.UserResponse))
                return BadRequest("Invalid request");

            var question = _botService.GetQuestionById(request.QuestionId);
            if (question == null)
                return BadRequest("Question not found");

            var response = new Response
            {
                QuestionId = request.QuestionId,
                QuestionText = question.Text,
                UserResponse = request.UserResponse,
                Category = question.Category,
                Person = question.Person,
                CreatedAt = DateTime.UtcNow,
                CoupleId = coupleId
            };

            _context.Responses.Add(response);
            await _context.SaveChangesAsync();

            return Ok(new { id = response.Id, category = response.Category });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, details = ex.InnerException?.Message });
        }
    }

    [HttpGet("responses")]
    public ActionResult<IEnumerable<Response>> GetResponses()
    {
        var coupleId = GetCoupleId();
        return Ok(_context.Responses
            .Where(r => r.CoupleId == coupleId)
            .OrderByDescending(r => r.CreatedAt)
            .ToList());
    }

    [HttpPut("responses/{id}")]
    public async Task<IActionResult> UpdateResponse(int id, [FromBody] Response response)
    {
        try
        {
            var coupleId = GetCoupleId();
            var existingResponse = await _context.Responses.FindAsync(id);
            if (existingResponse == null || existingResponse.CoupleId != coupleId)
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

    [HttpDelete("responses/{id}")]
    public async Task<IActionResult> DeleteResponse(int id)
    {
        try
        {
            var coupleId = GetCoupleId();
            var response = await _context.Responses.FindAsync(id);
            if (response == null || response.CoupleId != coupleId)
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
            if (!SharedConstants.Languages.Supported.Contains(language))
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
