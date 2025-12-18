using WodStrat.Dal.Enums;

namespace WodStrat.Dal.Models;

/// <summary>
/// Represents a parsed movement within a workout.
/// </summary>
public class WorkoutMovement
{
    /// <summary>
    /// Unique auto-incrementing identifier for the workout movement.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Reference to the parent workout.
    /// </summary>
    public int WorkoutId { get; set; }

    /// <summary>
    /// Reference to the canonical movement definition.
    /// </summary>
    public int MovementDefinitionId { get; set; }

    /// <summary>
    /// Order of the movement within the workout (1-indexed).
    /// </summary>
    public int SequenceOrder { get; set; }

    /// <summary>
    /// Number of repetitions.
    /// </summary>
    public int? RepCount { get; set; }

    /// <summary>
    /// Weight/load amount.
    /// </summary>
    public decimal? LoadValue { get; set; }

    /// <summary>
    /// Weight unit (kg/lb/pood).
    /// </summary>
    public LoadUnit? LoadUnit { get; set; }

    /// <summary>
    /// Distance amount.
    /// </summary>
    public decimal? DistanceValue { get; set; }

    /// <summary>
    /// Distance unit (m/km/ft/mi/cal).
    /// </summary>
    public DistanceUnit? DistanceUnit { get; set; }

    /// <summary>
    /// Calorie target (for erg machines).
    /// </summary>
    public int? Calories { get; set; }

    /// <summary>
    /// Time duration in seconds (for holds, etc.).
    /// </summary>
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// Additional specifications or modifications.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Starting minute for EMOM workouts (1-indexed).
    /// </summary>
    public int? MinuteStart { get; set; }

    /// <summary>
    /// Ending minute for EMOM workouts (for movements spanning multiple minutes).
    /// </summary>
    public int? MinuteEnd { get; set; }

    /// <summary>
    /// Record creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// The parent workout.
    /// </summary>
    public Workout Workout { get; set; } = null!;

    /// <summary>
    /// The canonical movement definition.
    /// </summary>
    public MovementDefinition MovementDefinition { get; set; } = null!;
}
