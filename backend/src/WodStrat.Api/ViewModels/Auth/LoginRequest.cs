namespace WodStrat.Api.ViewModels.Auth;

/// <summary>
/// Request model for user login.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// User's email address.
    /// </summary>
    /// <example>athlete@example.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's password.
    /// </summary>
    /// <example>MyP@ssw0rd!</example>
    public string Password { get; set; } = string.Empty;
}
