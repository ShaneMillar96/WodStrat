namespace WodStrat.Services.Dtos;

/// <summary>
/// Data transfer object for workout movement responses.
/// </summary>
public class WorkoutMovementDto
{
    /// <summary>
    /// Unique identifier for the workout movement.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Reference to the movement definition.
    /// </summary>
    public int MovementDefinitionId { get; set; }

    /// <summary>
    /// Display name of the movement.
    /// </summary>
    public string MovementName { get; set; } = string.Empty;

    /// <summary>
    /// Category of the movement.
    /// </summary>
    public string MovementCategory { get; set; } = string.Empty;

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
    /// Weight unit as string (kg/lb/pood).
    /// </summary>
    public string? LoadUnit { get; set; }

    /// <summary>
    /// Formatted load (e.g., "135 lb").
    /// </summary>
    public string? LoadFormatted { get; set; }

    /// <summary>
    /// Distance amount.
    /// </summary>
    public decimal? DistanceValue { get; set; }

    /// <summary>
    /// Distance unit as string (m/km/ft/mi/cal).
    /// </summary>
    public string? DistanceUnit { get; set; }

    /// <summary>
    /// Formatted distance (e.g., "400 m").
    /// </summary>
    public string? DistanceFormatted { get; set; }

    /// <summary>
    /// Calorie target.
    /// </summary>
    public int? Calories { get; set; }

    /// <summary>
    /// Time duration in seconds (for holds, etc.).
    /// </summary>
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// Formatted duration (e.g., "0:30").
    /// </summary>
    public string? DurationFormatted { get; set; }

    /// <summary>
    /// Additional notes or specifications.
    /// </summary>
    public string? Notes { get; set; }
}
