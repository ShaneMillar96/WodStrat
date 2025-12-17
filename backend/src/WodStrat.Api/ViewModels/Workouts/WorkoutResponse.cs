namespace WodStrat.Api.ViewModels.Workouts;

/// <summary>
/// Response model for workout data.
/// </summary>
public class WorkoutResponse
{
    /// <summary>
    /// Unique identifier for the workout.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// Optional workout name.
    /// </summary>
    /// <example>Cindy</example>
    public string? Name { get; set; }

    /// <summary>
    /// Workout type.
    /// </summary>
    /// <example>Amrap</example>
    public string WorkoutType { get; set; } = string.Empty;

    /// <summary>
    /// Raw text input from the user.
    /// </summary>
    /// <example>20 min AMRAP\n5 Pull-ups\n10 Push-ups\n15 Air Squats</example>
    public string OriginalText { get; set; } = string.Empty;

    /// <summary>
    /// Cleaned/normalized description.
    /// </summary>
    /// <example>AMRAP - 20 min - 3 movement(s)</example>
    public string? ParsedDescription { get; set; }

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
    /// List of movements in the workout.
    /// </summary>
    public IReadOnlyList<WorkoutMovementResponse> Movements { get; set; } = Array.Empty<WorkoutMovementResponse>();

    /// <summary>
    /// Timestamp when the workout was created.
    /// </summary>
    /// <example>2024-01-15T10:30:00Z</example>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the workout was last updated.
    /// </summary>
    /// <example>2024-01-15T10:30:00Z</example>
    public DateTime UpdatedAt { get; set; }
}
