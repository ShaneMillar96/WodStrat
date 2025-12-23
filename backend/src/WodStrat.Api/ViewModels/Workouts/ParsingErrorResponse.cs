namespace WodStrat.Api.ViewModels.Workouts;

/// <summary>
/// Response model for workout parsing errors.
/// </summary>
public class ParsingErrorResponse
{
    /// <summary>
    /// Machine-readable error code in SCREAMING_SNAKE_CASE.
    /// </summary>
    /// <example>NO_MOVEMENTS_DETECTED</example>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    /// <example>No workout movements were found in your input.</example>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Helpful suggestion for fixing the error.
    /// </summary>
    /// <example>Make sure each movement is on its own line with a rep count (e.g., '10 Pull-ups').</example>
    public string? Suggestion { get; set; }

    /// <summary>
    /// Line number where the error occurred (null for general errors).
    /// </summary>
    /// <example>3</example>
    public int? Line { get; set; }

    /// <summary>
    /// Error severity level.
    /// </summary>
    /// <example>error</example>
    public string Severity { get; set; } = "error";

    /// <summary>
    /// The original text that caused the error.
    /// </summary>
    /// <example>Unknown Exercise</example>
    public string? OriginalText { get; set; }
}
