namespace WodStrat.Api.ViewModels.Workouts;

/// <summary>
/// Response model for non-blocking parsing warnings.
/// </summary>
public class ParsingWarningResponse
{
    /// <summary>
    /// Machine-readable warning code in SCREAMING_SNAKE_CASE.
    /// </summary>
    /// <example>UNKNOWN_MOVEMENT</example>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable warning message.
    /// </summary>
    /// <example>Movement 'widowmakers' was not recognized.</example>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Helpful suggestion for resolving the warning.
    /// </summary>
    /// <example>Did you mean 'walking lunges'? You can also proceed and edit the movement name manually.</example>
    public string? Suggestion { get; set; }

    /// <summary>
    /// Line number where the warning applies (null for general warnings).
    /// </summary>
    /// <example>5</example>
    public int? Line { get; set; }

    /// <summary>
    /// Warning severity level.
    /// </summary>
    /// <example>warning</example>
    public string Severity { get; set; } = "warning";

    /// <summary>
    /// The original text that triggered the warning.
    /// </summary>
    /// <example>10 widowmakers</example>
    public string? OriginalText { get; set; }
}
