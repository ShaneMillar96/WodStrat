namespace WodStrat.Dal.Models;

/// <summary>
/// Represents a user account for authentication in WodStrat.
/// </summary>
public class User
{
    /// <summary>
    /// Unique auto-incrementing identifier for the user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// User email address (used for login).
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// BCrypt hashed password.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Account active status (false = deactivated).
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp when the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the user was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// The athlete profile associated with this user (one-to-one).
    /// </summary>
    public Athlete? Athlete { get; set; }
}
