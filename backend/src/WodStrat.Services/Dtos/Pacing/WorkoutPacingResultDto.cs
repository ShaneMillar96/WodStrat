namespace WodStrat.Services.Dtos;

/// <summary>
/// Complete pacing analysis result for a workout.
/// </summary>
public class WorkoutPacingResultDto
{
    /// <summary>
    /// Reference to the workout.
    /// </summary>
    public int WorkoutId { get; set; }

    /// <summary>
    /// Display name of the workout.
    /// </summary>
    public string WorkoutName { get; set; } = string.Empty;

    /// <summary>
    /// Type of workout (AMRAP, ForTime, etc.).
    /// </summary>
    public string WorkoutType { get; set; } = string.Empty;

    /// <summary>
    /// Per-movement pacing guidance.
    /// </summary>
    public IReadOnlyList<MovementPacingDto> MovementPacing { get; set; } = Array.Empty<MovementPacingDto>();

    /// <summary>
    /// Overall strategy notes for approaching the workout.
    /// </summary>
    public string OverallStrategyNotes { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the pacing was calculated.
    /// </summary>
    public DateTime CalculatedAt { get; set; }

    /// <summary>
    /// Summary counts by pacing level.
    /// </summary>
    public PacingDistributionDto Distribution { get; set; } = new();

    /// <summary>
    /// Whether all movements had sufficient data for accurate pacing.
    /// </summary>
    public bool IsComplete { get; set; }
}
