namespace WodStrat.Services.Dtos;

/// <summary>
/// Complete volume load analysis result for a workout.
/// </summary>
public class WorkoutVolumeLoadResultDto
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
    /// Per-movement volume load breakdown.
    /// </summary>
    public IReadOnlyList<MovementVolumeLoadDto> MovementVolumes { get; set; } = Array.Empty<MovementVolumeLoadDto>();

    /// <summary>
    /// Total volume load across all movements (sum of all MovementVolumeLoad values).
    /// </summary>
    public decimal TotalVolumeLoad { get; set; }

    /// <summary>
    /// Formatted total volume load string (e.g., "5,430 kg").
    /// </summary>
    public string TotalVolumeLoadFormatted { get; set; } = string.Empty;

    /// <summary>
    /// Overall assessment summary of workout load.
    /// </summary>
    public string OverallAssessment { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the calculation was performed.
    /// </summary>
    public DateTime CalculatedAt { get; set; }

    /// <summary>
    /// Distribution summary of load classifications.
    /// </summary>
    public VolumeLoadDistributionDto Distribution { get; set; } = new();

    /// <summary>
    /// Whether all movements had sufficient data for analysis.
    /// </summary>
    public bool IsComplete { get; set; }
}
