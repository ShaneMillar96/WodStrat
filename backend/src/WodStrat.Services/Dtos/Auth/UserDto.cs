namespace WodStrat.Services.Dtos.Auth;

/// <summary>
/// User profile information (excludes sensitive data like password hash).
/// </summary>
public class UserDto
{
    /// <summary>
    /// Unique user identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Whether the account is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Account creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
