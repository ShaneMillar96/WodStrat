using WodStrat.Dal.Enums;

namespace WodStrat.Services.Dtos;

/// <summary>
/// Data transfer object for updating an existing workout.
/// </summary>
public class UpdateWorkoutDto
{
    /// <summary>
    /// Optional workout name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Workout type (AMRAP/ForTime/EMOM/Intervals/Rounds).
    /// </summary>
    public WorkoutType WorkoutType { get; set; }

    /// <summary>
    /// Cleaned/normalized description.
    /// </summary>
    public string? ParsedDescription { get; set; }

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
    /// List of movements (replaces existing movements).
    /// </summary>
    public IList<CreateWorkoutMovementDto>? Movements { get; set; }
}
