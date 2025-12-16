using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WodStrat.Dal.Interfaces;
using WodStrat.Dal.Models;
using WodStrat.Services.Configuration;
using WodStrat.Services.Dtos.Auth;
using WodStrat.Services.Extensions;
using WodStrat.Services.Interfaces;

namespace WodStrat.Services.Services;

/// <summary>
/// Service implementation for authentication operations including registration and login.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IWodStratDatabase _database;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IWodStratDatabase database,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _database = database;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AuthResult<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        // Validate password confirmation
        if (dto.Password != dto.ConfirmPassword)
        {
            _logger.LogWarning("Registration failed: Password confirmation mismatch for email {Email}", dto.Email);
            return AuthResult<AuthResponseDto>.Fail(
                AuthErrorCodes.PasswordMismatch,
                "Password confirmation does not match.");
        }

        // Normalize email
        var normalizedEmail = dto.Email.ToLowerInvariant();

        // Check if email is already registered
        var existingUser = await _database.Get<User>()
            .Where(u => u.Email == normalizedEmail)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingUser != null)
        {
            _logger.LogWarning("Registration failed: Email already exists {Email}", normalizedEmail);
            return AuthResult<AuthResponseDto>.Fail(
                AuthErrorCodes.EmailExists,
                "An account with this email already exists.");
        }

        // Hash password using BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        // Create user entity
        var user = dto.ToEntity(passwordHash);

        _database.Add(user);
        await _database.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User registered successfully: {UserId}, {Email}", user.Id, user.Email);

        // Generate JWT token
        var token = GenerateJwtToken(user);
        var expiresInSeconds = _jwtSettings.ExpirationHours * 3600;

        return AuthResult<AuthResponseDto>.Succeed(new AuthResponseDto
        {
            Token = token,
            TokenType = "Bearer",
            ExpiresIn = expiresInSeconds,
            UserId = user.Id,
            Email = user.Email,
            HasAthleteProfile = false,
            AthleteId = null
        });
    }

    /// <inheritdoc />
    public async Task<AuthResult<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        // Normalize email
        var normalizedEmail = dto.Email.ToLowerInvariant();

        // Find user by email
        var user = await _database.Get<User>()
            .Include(u => u.Athlete)
            .Where(u => u.Email == normalizedEmail)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Login failed: User not found for email {Email}", normalizedEmail);
            // Use generic error message to prevent email enumeration
            return AuthResult<AuthResponseDto>.Fail(
                AuthErrorCodes.InvalidCredentials,
                "Invalid email or password.");
        }

        // Check if account is active
        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed: Account disabled for user {UserId}", user.Id);
            return AuthResult<AuthResponseDto>.Fail(
                AuthErrorCodes.AccountDisabled,
                "This account has been disabled.");
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: Invalid password for user {UserId}", user.Id);
            // Use generic error message to prevent password guessing
            return AuthResult<AuthResponseDto>.Fail(
                AuthErrorCodes.InvalidCredentials,
                "Invalid email or password.");
        }

        _logger.LogInformation("User logged in successfully: {UserId}, {Email}", user.Id, user.Email);

        // Check for athlete profile
        var hasAthleteProfile = user.Athlete != null && !user.Athlete.IsDeleted;
        var athleteId = hasAthleteProfile ? user.Athlete!.Id : (int?)null;

        // Generate JWT token with athleteId if user has an athlete profile
        var token = GenerateJwtToken(user, athleteId);
        var expiresInSeconds = _jwtSettings.ExpirationHours * 3600;

        return AuthResult<AuthResponseDto>.Succeed(new AuthResponseDto
        {
            Token = token,
            TokenType = "Bearer",
            ExpiresIn = expiresInSeconds,
            UserId = user.Id,
            Email = user.Email,
            HasAthleteProfile = hasAthleteProfile,
            AthleteId = hasAthleteProfile ? user.Athlete!.Id : null
        });
    }

    /// <inheritdoc />
    public async Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant();

        var existingUser = await _database.Get<User>()
            .Where(u => u.Email == normalizedEmail)
            .FirstOrDefaultAsync(cancellationToken);

        return existingUser == null;
    }

    /// <summary>
    /// Generates a JWT token for the authenticated user.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    /// <param name="athleteId">Optional athlete ID if user has an athlete profile.</param>
    /// <returns>The JWT token string.</returns>
    private string GenerateJwtToken(User user, int? athleteId = null)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Add athleteId claim if user has an athlete profile
        if (athleteId.HasValue)
        {
            claims.Add(new Claim("athleteId", athleteId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
