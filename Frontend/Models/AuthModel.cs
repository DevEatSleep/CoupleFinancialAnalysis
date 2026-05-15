namespace Frontend.Models;

public class AuthUser
{
    public int UserId { get; set; }
    public int CoupleId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
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

public class DeleteAccountRequest
{
    public string Token { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string Token { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
