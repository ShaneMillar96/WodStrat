using WodStrat.Services.Dtos.Auth;

namespace WodStrat.Services.Interfaces;

/// <summary>
/// Service interface for authentication operations including registration and login.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user account with email and password.
    /// Creates the user record with hashed password.
    /// </summary>
    /// <param name="dto">Registration data containing email and password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>AuthResponseDto with JWT token, or error if email already exists.</returns>
    Task<AuthResult<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="dto">Login credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>AuthResponseDto with JWT token if successful, or error details.</returns>
    Task<AuthResult<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if an email address is available for registration.
    /// </summary>
    /// <param name="email">Email to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if email is available, false if already registered.</returns>
    Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default);
}
