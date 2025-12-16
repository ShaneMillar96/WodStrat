namespace WodStrat.Api.ViewModels.Auth;

/// <summary>
/// Request model for user registration.
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// User's email address (used as username).
    /// </summary>
    /// <example>athlete@example.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Password for the account. Must be at least 8 characters,
    /// contain at least 1 uppercase letter, and 1 special character.
    /// </summary>
    /// <example>MyP@ssw0rd!</example>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Password confirmation (must match Password).
    /// </summary>
    /// <example>MyP@ssw0rd!</example>
    public string ConfirmPassword { get; set; } = string.Empty;
}
