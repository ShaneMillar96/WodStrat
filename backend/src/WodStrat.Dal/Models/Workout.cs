using WodStrat.Dal.Enums;

namespace WodStrat.Dal.Models;

/// <summary>
/// Represents a user workout with parsed structure.
/// </summary>
public class Workout : EntityBase
{
    /// <summary>
    /// Reference to the user who owns this workout.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Optional workout name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Workout format (amrap/for_time/emom/intervals/rounds).
    /// </summary>
    public WorkoutType WorkoutType { get; set; }

    /// <summary>
    /// Raw text input from the user.
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
    /// Interval duration in seconds (for EMOM).
    /// </summary>
    public int? IntervalDurationSeconds { get; set; }

    // Navigation properties
    /// <summary>
    /// The user who owns this workout.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Collection of movements in this workout.
    /// </summary>
    public ICollection<WorkoutMovement> Movements { get; set; } = new List<WorkoutMovement>();
}
