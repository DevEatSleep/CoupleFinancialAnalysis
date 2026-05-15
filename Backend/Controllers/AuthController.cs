using CoupleChat.Data;
using CoupleChat.Models;
using CoupleChat.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoupleChat.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ChatDbContext _context;
    private readonly JwtService _jwtService;
    private readonly PasswordService _passwordService;

    public AuthController(ChatDbContext context, JwtService jwtService, PasswordService passwordService)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordService = passwordService;
    }

    /// <summary>
    /// Register a new couple with two users
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Name1) || 
            string.IsNullOrWhiteSpace(request.Name2))
            return BadRequest("Email, password, and both names are required");

        // Check if email already exists
        if (_context.Users.Any(u => u.Email == request.Email))
            return BadRequest("Email already in use");

        // Create new couple
        var couple = new Couple();
        _context.Couples.Add(couple);
        await _context.SaveChangesAsync();

        // Hash password
        var passwordHash = _passwordService.HashPassword(request.Password);

        // Create first user
        var user1 = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            Name = request.Name1,
            CoupleId = couple.Id,
            CreatedAt = DateTime.UtcNow
        };

        // Create second user (optional)
        User? user2 = null;
        if (!string.IsNullOrWhiteSpace(request.Email2))
        {
            if (_context.Users.Any(u => u.Email == request.Email2))
                return BadRequest("Email2 already in use");

            user2 = new User
            {
                Email = request.Email2,
                PasswordHash = passwordHash, // Same password initially
                Name = request.Name2,
                CoupleId = couple.Id,
                CreatedAt = DateTime.UtcNow
            };
        }

        _context.Users.Add(user1);
        if (user2 != null)
            _context.Users.Add(user2);

        await _context.SaveChangesAsync();

        // Generate token
        var token = _jwtService.GenerateToken(user1.Id, couple.Id, user1.Email);

        return Ok(new AuthResponse
        {
            Token = token,
            UserId = user1.Id,
            CoupleId = couple.Id,
            Email = user1.Email,
            Name = user1.Name,
            WomanName = user1.Name,
            ManName = user2?.Name ?? user1.Name
        });
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    public ActionResult<AuthResponse> Login([FromBody] LoginRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email and password are required");

        var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
        if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            return Unauthorized("Invalid email or password");

        // Get the couple's users to retrieve both names
        var coupleUsers = _context.Users.Where(u => u.CoupleId == user.CoupleId).OrderBy(u => u.Id).ToList();
        var womanName = coupleUsers.Count > 0 ? coupleUsers[0].Name : user.Name;
        var manName = coupleUsers.Count > 1 ? coupleUsers[1].Name : user.Name;

        // Generate token
        var token = _jwtService.GenerateToken(user.Id, user.CoupleId, user.Email);

        return Ok(new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            CoupleId = user.CoupleId,
            Email = user.Email,
            Name = user.Name,
            WomanName = womanName,
            ManName = manName
        });
    }

    /// <summary>
    /// Verify if a token is still valid
    /// </summary>
    [HttpPost("verify")]
    public ActionResult<AuthResponse> VerifyToken([FromBody] VerifyTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return BadRequest("Token is required");

        var principal = _jwtService.ValidateToken(request.Token);
        if (principal == null)
            return Unauthorized("Invalid or expired token");

        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var coupleIdClaim = principal.FindFirst("coupleId")?.Value;
        var emailClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        if (!int.TryParse(userIdClaim, out var userId) || !int.TryParse(coupleIdClaim, out var coupleId))
            return Unauthorized("Invalid token claims");

        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return Unauthorized("User not found");

        // Get the couple's users to retrieve both names
        var coupleUsers = _context.Users.Where(u => u.CoupleId == coupleId).OrderBy(u => u.Id).ToList();
        var womanName = coupleUsers.Count > 0 ? coupleUsers[0].Name : user.Name;
        var manName = coupleUsers.Count > 1 ? coupleUsers[1].Name : user.Name;

        return Ok(new AuthResponse
        {
            Token = request.Token,
            UserId = user.Id,
            CoupleId = user.CoupleId,
            Email = user.Email,
            Name = user.Name,
            WomanName = womanName,
            ManName = manName
        });
    }

    /// <summary>
    /// Change password for the currently authenticated user
    /// </summary>
    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest("Token, current password, and new password are required");

        var principal = _jwtService.ValidateToken(request.Token);
        if (principal == null)
            return Unauthorized("Invalid or expired token");

        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized("Invalid token claims");

        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return Unauthorized("User not found");

        if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            return Unauthorized("Current password is incorrect");

        user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Password changed successfully" });
    }

    /// <summary>
    /// Delete the entire couple account and all associated data
    /// </summary>
    [HttpDelete("delete-account")]
    public async Task<ActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Token and password are required");

        var principal = _jwtService.ValidateToken(request.Token);
        if (principal == null)
            return Unauthorized("Invalid or expired token");

        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var coupleIdClaim = principal.FindFirst("coupleId")?.Value;

        if (!int.TryParse(userIdClaim, out var userId) || !int.TryParse(coupleIdClaim, out var coupleId))
            return Unauthorized("Invalid token claims");

        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return Unauthorized("User not found");

        // Verify password
        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            return Unauthorized("Invalid password");

        // Delete all data for this couple
        try
        {
            // Delete chat messages
            var messages = _context.Messages.Where(m => m.CoupleId == coupleId).ToList();
            _context.Messages.RemoveRange(messages);

            // Delete expenses
            var expenses = _context.Expenses.Where(e => e.CoupleId == coupleId).ToList();
            _context.Expenses.RemoveRange(expenses);

            // Delete travail domestique data
            var domestiqueData = _context.TravailDomestique.Where(t => t.CoupleId == coupleId).ToList();
            _context.TravailDomestique.RemoveRange(domestiqueData);

            // Delete all users in this couple
            var coupleUsers = _context.Users.Where(u => u.CoupleId == coupleId).ToList();
            _context.Users.RemoveRange(coupleUsers);

            // Delete the couple
            var couple = _context.Couples.FirstOrDefault(c => c.Id == coupleId);
            if (couple != null)
                _context.Couples.Remove(couple);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Account deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error deleting account: " + ex.Message });
        }
    }
}

public class ChangePasswordRequest
{
    public string Token { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class DeleteAccountRequest
{
    public string Token { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Email2 { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name1 { get; set; } = string.Empty;
    public string Name2 { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class VerifyTokenRequest
{
    public string Token { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int CoupleId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string WomanName { get; set; } = string.Empty;
    public string ManName { get; set; } = string.Empty;
}
