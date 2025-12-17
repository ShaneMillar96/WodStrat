namespace WodStrat.Api.ViewModels.Workouts;

/// <summary>
/// Request model for updating an existing workout.
/// </summary>
public class UpdateWorkoutRequest
{
    /// <summary>
    /// Optional name for the workout.
    /// </summary>
    /// <example>Cindy Modified</example>
    public string? Name { get; set; }

    /// <summary>
    /// The workout type (Amrap, ForTime, Emom, Intervals, Rounds).
    /// </summary>
    /// <example>Amrap</example>
    public string WorkoutType { get; set; } = string.Empty;

    /// <summary>
    /// Time cap in seconds (for timed workouts).
    /// </summary>
    /// <example>1200</example>
    public int? TimeCapSeconds { get; set; }

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
    /// List of movements (replaces existing movements).
    /// </summary>
    public IList<CreateWorkoutMovementRequest>? Movements { get; set; }
}
