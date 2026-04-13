using CoupleChat.Data;
using CoupleChat.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoupleChat.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ChatDbContext _context;

    public ChatController(ChatDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Message>> GetMessages()
    {
        return Ok(_context.Messages.OrderBy(m => m.CreatedAt).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<Message>> PostMessage([FromBody] Message message)
    {
        if (string.IsNullOrEmpty(message.Sender) || string.IsNullOrEmpty(message.Content))
            return BadRequest("Sender and Content are required");

        message.CreatedAt = DateTime.UtcNow;
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMessages), new { id = message.Id }, message);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        var message = await _context.Messages.FindAsync(id);
        if (message == null)
            return NotFound();

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
