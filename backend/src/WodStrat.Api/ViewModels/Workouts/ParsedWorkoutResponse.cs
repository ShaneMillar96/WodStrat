namespace WodStrat.Api.ViewModels.Workouts;

/// <summary>
/// Response model for parsed workout text results.
/// </summary>
public class ParsedWorkoutResponse
{
    /// <summary>
    /// Original raw text that was parsed.
    /// </summary>
    /// <example>20 min AMRAP\n10 Pull-ups\n15 Push-ups\n20 Air Squats</example>
    public string OriginalText { get; set; } = string.Empty;

    /// <summary>
    /// Cleaned/normalized description after parsing.
    /// </summary>
    /// <example>AMRAP - 20 min - 3 movement(s)</example>
    public string? ParsedDescription { get; set; }

    /// <summary>
    /// Detected workout type.
    /// </summary>
    /// <example>Amrap</example>
    public string WorkoutType { get; set; } = string.Empty;

    /// <summary>
    /// Time cap in seconds (for timed workouts).
    /// </summary>
    /// <example>1200</example>
    public int? TimeCapSeconds { get; set; }

    /// <summary>
    /// Formatted time cap (e.g., "20:00").
    /// </summary>
    /// <example>20:00</example>
    public string? TimeCapFormatted { get; set; }

    /// <summary>
    /// Number of rounds (for round-based workouts).
    /// </summary>
    /// <example>5</example>
    public int? RoundCount { get; set; }

    /// <summary>
    /// Interval duration in seconds (for EMOM workouts).
    /// </summary>
    /// <example>60</example>
    public int? IntervalDurationSeconds { get; set; }

    /// <summary>
    /// Formatted interval duration (e.g., "1:00").
    /// </summary>
    /// <example>1:00</example>
    public string? IntervalDurationFormatted { get; set; }

    /// <summary>
    /// List of parsed movements in order.
    /// </summary>
    public IReadOnlyList<ParsedMovementResponse> Movements { get; set; } = Array.Empty<ParsedMovementResponse>();

    /// <summary>
    /// Parse confidence score (0.0 to 1.0). Included for convenience.
    /// </summary>
    /// <example>0.85</example>
    public decimal? ParseConfidence { get; set; }
}
