namespace WodStrat.Services.Dtos.Auth;

/// <summary>
/// Data transfer object for login request credentials.
/// </summary>
public class LoginDto
{
    /// <summary>
    /// User's email address for authentication.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's password (plaintext, will be verified against hash).
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
