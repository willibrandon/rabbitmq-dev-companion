using System.ComponentModel.DataAnnotations;

namespace Companion.Core.Models.Auth;

public record LoginRequest(
    [Required] string Username,
    [Required] string Password
);

public record LoginResponse(
    string Token,
    string Username,
    string Role,
    DateTime ExpiresAt
);

public class RegisterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public record RegisterResponse(
    string Username,
    string Email,
    string Role
); 