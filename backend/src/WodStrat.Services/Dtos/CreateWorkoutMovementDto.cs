using WodStrat.Dal.Enums;

namespace WodStrat.Services.Dtos;

/// <summary>
/// Data transfer object for creating a workout movement.
/// </summary>
public class CreateWorkoutMovementDto
{
    /// <summary>
    /// Reference to the movement definition. Required.
    /// </summary>
    public int MovementDefinitionId { get; set; }

    /// <summary>
    /// Order of the movement within the workout (1-indexed). Required.
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
    /// Calorie target.
    /// </summary>
    public int? Calories { get; set; }

    /// <summary>
    /// Time duration in seconds (for holds, etc.).
    /// </summary>
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// Additional notes or specifications.
    /// </summary>
    public string? Notes { get; set; }
}
