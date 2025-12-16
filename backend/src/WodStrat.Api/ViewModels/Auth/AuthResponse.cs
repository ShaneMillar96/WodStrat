namespace WodStrat.Api.ViewModels.Auth;

/// <summary>
/// Response model for successful authentication.
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// JWT access token for API authorization.
    /// </summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Token type (always "Bearer").
    /// </summary>
    /// <example>Bearer</example>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Token expiration time in seconds.
    /// </summary>
    /// <example>86400</example>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// User's email address.
    /// </summary>
    /// <example>athlete@example.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's unique identifier.
    /// </summary>
    /// <example>42</example>
    public int UserId { get; set; }

    /// <summary>
    /// Whether the user has an associated athlete profile.
    /// </summary>
    /// <example>true</example>
    public bool HasAthleteProfile { get; set; }

    /// <summary>
    /// The athlete profile ID if one exists.
    /// </summary>
    /// <example>1</example>
    public int? AthleteId { get; set; }
}
