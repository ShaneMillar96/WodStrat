namespace WodStrat.Api.ViewModels.Workouts;

/// <summary>
/// Response model for workout movement data.
/// </summary>
public class WorkoutMovementResponse
{
    /// <summary>
    /// Unique identifier for the workout movement.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// Reference to the movement definition.
    /// </summary>
    /// <example>15</example>
    public int MovementDefinitionId { get; set; }

    /// <summary>
    /// Display name of the movement.
    /// </summary>
    /// <example>Pull-Up</example>
    public string MovementName { get; set; } = string.Empty;

    /// <summary>
    /// Category of the movement.
    /// </summary>
    /// <example>Gymnastics</example>
    public string MovementCategory { get; set; } = string.Empty;

    /// <summary>
    /// Order of the movement within the workout (1-indexed).
    /// </summary>
    /// <example>1</example>
    public int SequenceOrder { get; set; }

    /// <summary>
    /// Number of repetitions.
    /// </summary>
    /// <example>10</example>
    public int? RepCount { get; set; }

    /// <summary>
    /// Weight/load amount.
    /// </summary>
    /// <example>135</example>
    public decimal? LoadValue { get; set; }

    /// <summary>
    /// Weight unit.
    /// </summary>
    /// <example>Lb</example>
    public string? LoadUnit { get; set; }

    /// <summary>
    /// Formatted load (e.g., "135 lb").
    /// </summary>
    /// <example>135 lb</example>
    public string? LoadFormatted { get; set; }

    /// <summary>
    /// Distance amount.
    /// </summary>
    /// <example>400</example>
    public decimal? DistanceValue { get; set; }

    /// <summary>
    /// Distance unit.
    /// </summary>
    /// <example>M</example>
    public string? DistanceUnit { get; set; }

    /// <summary>
    /// Formatted distance (e.g., "400 m").
    /// </summary>
    /// <example>400 m</example>
    public string? DistanceFormatted { get; set; }

    /// <summary>
    /// Calorie target.
    /// </summary>
    /// <example>15</example>
    public int? Calories { get; set; }

    /// <summary>
    /// Time duration in seconds (for holds, etc.).
    /// </summary>
    /// <example>30</example>
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// Formatted duration (e.g., "0:30").
    /// </summary>
    /// <example>0:30</example>
    public string? DurationFormatted { get; set; }

    /// <summary>
    /// Additional notes or specifications.
    /// </summary>
    public string? Notes { get; set; }
}
