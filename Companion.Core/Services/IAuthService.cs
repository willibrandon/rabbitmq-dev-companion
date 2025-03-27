using Companion.Core.Models.Auth;

namespace Companion.Core.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<bool> ValidateTokenAsync(string token);
} 