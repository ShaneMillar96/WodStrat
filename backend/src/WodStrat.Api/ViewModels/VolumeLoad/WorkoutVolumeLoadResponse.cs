namespace WodStrat.Api.ViewModels.VolumeLoad;

/// <summary>
/// Response model for workout volume load analysis.
/// </summary>
public class WorkoutVolumeLoadResponse
{
    /// <summary>
    /// The workout's unique identifier.
    /// </summary>
    /// <example>5</example>
    public int WorkoutId { get; set; }

    /// <summary>
    /// Optional workout name.
    /// </summary>
    /// <example>DT</example>
    public string? WorkoutName { get; set; }

    /// <summary>
    /// Per-movement volume load breakdown.
    /// </summary>
    public List<MovementVolumeLoadResponse> MovementVolumes { get; set; } = new();

    /// <summary>
    /// Total volume load across all movements (sum of weight x reps x rounds).
    /// </summary>
    /// <example>5430.00</example>
    public decimal TotalVolumeLoad { get; set; }

    /// <summary>
    /// Human-readable formatted total volume load.
    /// </summary>
    /// <example>5,430 kg</example>
    public string TotalVolumeLoadFormatted { get; set; } = string.Empty;

    /// <summary>
    /// Overall assessment summary of the workout's volume load relative to athlete capabilities.
    /// </summary>
    /// <example>This workout has moderate total volume. Your deadlifts are at a challenging weight - consider scaling to 80% (120 kg) for better sustainability.</example>
    public string OverallAssessment { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the volume load was calculated.
    /// </summary>
    /// <example>2024-01-15T10:30:00Z</example>
    public DateTime CalculatedAt { get; set; }
}
