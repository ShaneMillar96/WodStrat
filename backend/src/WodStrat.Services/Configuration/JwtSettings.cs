namespace WodStrat.Services.Configuration;

/// <summary>
/// Strongly-typed JWT configuration settings.
/// Maps to the "Jwt" section in appsettings.json.
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Secret key for signing JWT tokens (minimum 32 characters).
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// JWT issuer (typically the API URL).
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// JWT audience (typically the client app).
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration in hours. Default: 24.
    /// </summary>
    public int ExpirationHours { get; set; } = 24;
}
