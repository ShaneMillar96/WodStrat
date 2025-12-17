namespace WodStrat.Api.ViewModels.Workouts;

/// <summary>
/// Request model for creating a new workout.
/// </summary>
public class CreateWorkoutRequest
{
    /// <summary>
    /// Optional name for the workout (e.g., "Cindy", "My Custom WOD").
    /// </summary>
    /// <example>Cindy</example>
    public string? Name { get; set; }

    /// <summary>
    /// The workout type (Amrap, ForTime, Emom, Intervals, Rounds).
    /// </summary>
    /// <example>Amrap</example>
    public string WorkoutType { get; set; } = string.Empty;

    /// <summary>
    /// The original raw text input.
    /// </summary>
    /// <example>20 min AMRAP\n10 Pull-ups\n15 Push-ups\n20 Air Squats</example>
    public string OriginalText { get; set; } = string.Empty;

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
    /// List of movements in the workout.
    /// </summary>
    public IList<CreateWorkoutMovementRequest> Movements { get; set; } = new List<CreateWorkoutMovementRequest>();
}
