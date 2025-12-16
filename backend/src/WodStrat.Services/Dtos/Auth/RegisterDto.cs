namespace WodStrat.Services.Dtos.Auth;

/// <summary>
/// Data transfer object for user registration.
/// </summary>
public class RegisterDto
{
    /// <summary>
    /// Email address for the new account. Must be unique.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Password for the new account.
    /// Must meet complexity requirements (8+ chars, 1 uppercase, 1 special).
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Password confirmation. Must match Password field.
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
}
