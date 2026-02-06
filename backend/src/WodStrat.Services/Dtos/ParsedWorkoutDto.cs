using WodStrat.Dal.Enums;

namespace WodStrat.Services.Dtos;

/// <summary>
/// Data transfer object for parsed workout text results.
/// Contains the structured data extracted from raw workout text.
/// </summary>
public class ParsedWorkoutDto
{
    /// <summary>
    /// Original raw text that was parsed.
    /// </summary>
    public string OriginalText { get; set; } = string.Empty;

    /// <summary>
    /// Cleaned/normalized description after parsing.
    /// </summary>
    public string? ParsedDescription { get; set; }

    /// <summary>
    /// Detected workout type (AMRAP/ForTime/EMOM/Intervals/Rounds).
    /// </summary>
    public WorkoutType WorkoutType { get; set; }

    /// <summary>
    /// Time cap in seconds (for timed workouts).
    /// </summary>
    public int? TimeCapSeconds { get; set; }

    /// <summary>
    /// Number of rounds (for round-based workouts).
    /// </summary>
    public int? RoundCount { get; set; }

    /// <summary>
    /// Interval duration in seconds (for EMOM workouts).
    /// </summary>
    public int? IntervalDurationSeconds { get; set; }

    /// <summary>
    /// List of parsed movements in order.
    /// </summary>
    public IList<ParsedMovementDto> Movements { get; set; } = new List<ParsedMovementDto>();

    /// <summary>
    /// List of parsing errors encountered.
    /// </summary>
    public IList<ParsingErrorDto> Errors { get; set; } = new List<ParsingErrorDto>();

    /// <summary>
    /// Indicates if parsing completed successfully without errors.
    /// </summary>
    public bool IsValid => Errors.Count == 0 && Movements.Count > 0;

    /// <summary>
    /// Workout-level rep scheme reps (e.g., [21, 15, 9] for "21-15-9").
    /// Applies to all movements unless overridden at movement level.
    /// </summary>
    public int[]? RepSchemeReps { get; set; }

    /// <summary>
    /// Type of workout-level rep scheme (Descending, Ascending, Fixed, Custom).
    /// </summary>
    public string? RepSchemeType { get; set; }
}
