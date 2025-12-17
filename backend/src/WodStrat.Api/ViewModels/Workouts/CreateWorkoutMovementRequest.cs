namespace WodStrat.Api.ViewModels.Workouts;

/// <summary>
/// Request model for creating a workout movement.
/// </summary>
public class CreateWorkoutMovementRequest
{
    /// <summary>
    /// Reference to the movement definition.
    /// </summary>
    /// <example>15</example>
    public int MovementDefinitionId { get; set; }

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
    /// Weight unit (Kg, Lb, Pood).
    /// </summary>
    /// <example>Lb</example>
    public string? LoadUnit { get; set; }

    /// <summary>
    /// Distance amount.
    /// </summary>
    /// <example>400</example>
    public decimal? DistanceValue { get; set; }

    /// <summary>
    /// Distance unit (M, Km, Ft, Mi, Cal).
    /// </summary>
    /// <example>M</example>
    public string? DistanceUnit { get; set; }

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
    /// Additional notes or specifications.
    /// </summary>
    /// <example>strict</example>
    public string? Notes { get; set; }
}
