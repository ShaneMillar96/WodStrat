namespace WodStrat.Services.Dtos.Auth;

/// <summary>
/// JWT authentication response containing token and user info.
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// JWT access token for API authentication.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Token type (always "Bearer").
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Token expiration time in seconds.
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Authenticated user's ID.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Authenticated user's email.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Whether the user has an associated athlete profile.
    /// </summary>
    public bool HasAthleteProfile { get; set; }

    /// <summary>
    /// The athlete profile ID if one exists.
    /// </summary>
    public int? AthleteId { get; set; }
}
