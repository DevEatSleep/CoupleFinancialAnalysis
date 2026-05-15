using CoupleChat.Data;
using CoupleChat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoupleChat.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly ChatDbContext _context;

    public ChatController(ChatDbContext context)
    {
        _context = context;
    }

    private int GetCoupleId()
    {
        var coupleIdObj = HttpContext.Items["CoupleId"];
        if (coupleIdObj is int coupleId)
            return coupleId;

        throw new UnauthorizedAccessException("CoupleId not found in token");
    }

    [HttpGet]
    public ActionResult<IEnumerable<Message>> GetMessages()
    {
        var coupleId = GetCoupleId();
        return Ok(_context.Messages
            .Where(m => m.CoupleId == coupleId)
            .OrderBy(m => m.CreatedAt)
            .ToList());
    }

    [HttpPost]
    public async Task<ActionResult<Message>> PostMessage([FromBody] Message message)
    {
        var coupleId = GetCoupleId();

        if (string.IsNullOrEmpty(message.Sender) || string.IsNullOrEmpty(message.Content))
            return BadRequest("Sender and Content are required");

        message.CreatedAt = DateTime.UtcNow;
        message.CoupleId = coupleId;
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMessages), new { id = message.Id }, message);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        var coupleId = GetCoupleId();
        var message = await _context.Messages.FindAsync(id);
        if (message == null || message.CoupleId != coupleId)
            return NotFound();

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
