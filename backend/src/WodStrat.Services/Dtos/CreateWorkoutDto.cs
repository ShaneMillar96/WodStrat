using WodStrat.Dal.Enums;

namespace WodStrat.Services.Dtos;

/// <summary>
/// Data transfer object for creating a new workout.
/// Can be populated from parsing results or manual input.
/// </summary>
public class CreateWorkoutDto
{
    /// <summary>
    /// Optional workout name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Workout type (AMRAP/ForTime/EMOM/Intervals/Rounds). Required.
    /// </summary>
    public WorkoutType WorkoutType { get; set; }

    /// <summary>
    /// Raw text input from the user. Required.
    /// </summary>
    public string OriginalText { get; set; } = string.Empty;

    /// <summary>
    /// Cleaned/normalized description after parsing.
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
    /// List of movements to create with the workout.
    /// </summary>
    public IList<CreateWorkoutMovementDto> Movements { get; set; } = new List<CreateWorkoutMovementDto>();
}
