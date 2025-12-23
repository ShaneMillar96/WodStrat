namespace WodStrat.Api.ViewModels.Workouts;

/// <summary>
/// Response model for workout parsing results with error handling.
/// </summary>
public class ParsedWorkoutResultResponse
{
    /// <summary>
    /// Whether parsing completed successfully with acceptable confidence.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// List of blocking parsing errors.
    /// </summary>
    public IReadOnlyList<ParsingErrorResponse> Errors { get; set; } = Array.Empty<ParsingErrorResponse>();

    /// <summary>
    /// List of non-blocking parsing warnings.
    /// </summary>
    public IReadOnlyList<ParsingWarningResponse> Warnings { get; set; } = Array.Empty<ParsingWarningResponse>();

    /// <summary>
    /// The fully parsed workout (null if parsing failed completely).
    /// </summary>
    public ParsedWorkoutResponse? ParsedWorkout { get; set; }

    /// <summary>
    /// Partial result for recoverable scenarios (may contain incomplete data).
    /// </summary>
    public ParsedWorkoutResponse? PartialResult { get; set; }

    /// <summary>
    /// Overall confidence score (0.0 to 1.0).
    /// </summary>
    /// <example>0.85</example>
    public decimal ParseConfidence { get; set; }

    /// <summary>
    /// Human-readable confidence level.
    /// </summary>
    /// <example>High</example>
    public string ConfidenceLevel { get; set; } = string.Empty;

    /// <summary>
    /// Detailed breakdown of confidence factors.
    /// </summary>
    public ConfidenceBreakdownResponse? ConfidenceDetails { get; set; }

    /// <summary>
    /// Whether the result is usable despite warnings.
    /// </summary>
    /// <example>true</example>
    public bool IsUsable { get; set; }
}
