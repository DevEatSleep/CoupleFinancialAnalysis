using CoupleChat.Data;
using CoupleChat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoupleChat.Controllers;

[ApiController]
[Route("api/expenses")]
[Authorize]
public class ExpenseController : ControllerBase
{
    private readonly ChatDbContext _context;

    public ExpenseController(ChatDbContext context)
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

    [HttpPost]
    public async Task<ActionResult<Expense>> PostExpense([FromBody] Expense expense)
    {
        try
        {
            var coupleId = GetCoupleId();

            if (string.IsNullOrEmpty(expense.Label) || expense.Amount <= 0 || string.IsNullOrEmpty(expense.PaidBy))
                return BadRequest("Label, Amount, and PaidBy are required");

            expense.CreatedAt = DateTime.UtcNow;
            expense.CoupleId = coupleId;
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return Ok(expense);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet]
    public ActionResult<IEnumerable<Expense>> GetExpenses()
    {
        var coupleId = GetCoupleId();
        return Ok(_context.Expenses
            .Where(e => e.CoupleId == coupleId)
            .OrderByDescending(e => e.CreatedAt)
            .ToList());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExpense(int id, [FromBody] Expense expense)
    {
        try
        {
            var coupleId = GetCoupleId();
            var existingExpense = await _context.Expenses.FindAsync(id);
            if (existingExpense == null || existingExpense.CoupleId != coupleId)
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        try
        {
            var coupleId = GetCoupleId();
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null || expense.CoupleId != coupleId)
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
}
