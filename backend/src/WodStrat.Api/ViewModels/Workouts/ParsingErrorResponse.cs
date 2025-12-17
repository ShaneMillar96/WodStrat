namespace WodStrat.Api.ViewModels.Workouts;

/// <summary>
/// Response model for workout parsing errors.
/// </summary>
public class ParsingErrorResponse
{
    /// <summary>
    /// Type of error encountered.
    /// </summary>
    /// <example>UnrecognizedMovement</example>
    public string ErrorType { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    /// <example>Could not parse movement: 'Unknown Exercise'</example>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Line number where the error occurred (0 for general errors).
    /// </summary>
    /// <example>3</example>
    public int LineNumber { get; set; }

    /// <summary>
    /// The original text that caused the error.
    /// </summary>
    /// <example>Unknown Exercise</example>
    public string? OriginalText { get; set; }
}
