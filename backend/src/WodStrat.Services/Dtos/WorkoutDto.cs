namespace WodStrat.Services.Dtos;

/// <summary>
/// Data transfer object for workout responses.
/// </summary>
public class WorkoutDto
{
    /// <summary>
    /// Unique identifier for the workout.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Reference to the user who owns this workout.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Optional workout name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Workout type as string (AMRAP/ForTime/EMOM/Intervals/Rounds).
    /// </summary>
    public string WorkoutType { get; set; } = string.Empty;

    /// <summary>
    /// Raw text input from the user.
    /// </summary>
    public string OriginalText { get; set; } = string.Empty;

    /// <summary>
    /// Cleaned/normalized description.
    /// </summary>
    public string? ParsedDescription { get; set; }

    /// <summary>
    /// Time cap in seconds (for timed workouts).
    /// </summary>
    public int? TimeCapSeconds { get; set; }

    /// <summary>
    /// Formatted time cap (e.g., "20:00").
    /// </summary>
    public string? TimeCapFormatted { get; set; }

    /// <summary>
    /// Number of rounds (for round-based workouts).
    /// </summary>
    public int? RoundCount { get; set; }

    /// <summary>
    /// Interval duration in seconds (for EMOM workouts).
    /// </summary>
    public int? IntervalDurationSeconds { get; set; }

    /// <summary>
    /// Formatted interval duration (e.g., "1:00").
    /// </summary>
    public string? IntervalDurationFormatted { get; set; }

    /// <summary>
    /// List of movements in the workout.
    /// </summary>
    public IReadOnlyList<WorkoutMovementDto> Movements { get; set; } = Array.Empty<WorkoutMovementDto>();

    /// <summary>
    /// Timestamp when the workout was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the workout was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
