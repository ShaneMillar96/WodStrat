namespace WodStrat.Api.ViewModels.VolumeLoad;

/// <summary>
/// Response model for individual movement volume load analysis.
/// </summary>
public class MovementVolumeLoadResponse
{
    /// <summary>
    /// Reference to the movement definition.
    /// </summary>
    /// <example>15</example>
    public int MovementDefinitionId { get; set; }

    /// <summary>
    /// Display name of the movement.
    /// </summary>
    /// <example>Deadlift</example>
    public string MovementName { get; set; } = string.Empty;

    /// <summary>
    /// Weight used for this movement.
    /// </summary>
    /// <example>102.5</example>
    public decimal Weight { get; set; }

    /// <summary>
    /// Unit of weight measurement.
    /// </summary>
    /// <example>kg</example>
    public string WeightUnit { get; set; } = string.Empty;

    /// <summary>
    /// Number of repetitions per round.
    /// </summary>
    /// <example>12</example>
    public int Reps { get; set; }

    /// <summary>
    /// Number of rounds in the workout.
    /// </summary>
    /// <example>5</example>
    public int Rounds { get; set; }

    /// <summary>
    /// Calculated volume load (weight x reps x rounds).
    /// </summary>
    /// <example>6150.00</example>
    public decimal VolumeLoad { get; set; }

    /// <summary>
    /// Human-readable formatted volume load.
    /// </summary>
    /// <example>6,150 kg</example>
    public string VolumeLoadFormatted { get; set; } = string.Empty;

    /// <summary>
    /// Load classification based on athlete's strength benchmarks and experience.
    /// </summary>
    /// <example>High</example>
    public string LoadClassification { get; set; } = string.Empty;

    /// <summary>
    /// The benchmark that was used to determine load classification.
    /// </summary>
    /// <example>1 Rep Max Deadlift</example>
    public string? BenchmarkUsed { get; set; }

    /// <summary>
    /// The athlete's percentile ranking for this movement type (0.0 to 1.0).
    /// Null if no benchmark data available.
    /// </summary>
    /// <example>0.72</example>
    public decimal? AthleteBenchmarkPercentile { get; set; }

    /// <summary>
    /// Actionable tip for this movement including scaling recommendations.
    /// </summary>
    /// <example>This weight is 85% of your 1RM. Consider scaling to 70% (71 kg) for better workout sustainability.</example>
    public string Tip { get; set; } = string.Empty;

    /// <summary>
    /// Recommended scaled weight if the load is classified as high.
    /// Null if no scaling recommended.
    /// </summary>
    /// <example>71.0</example>
    public decimal? RecommendedWeight { get; set; }

    /// <summary>
    /// Human-readable recommended weight with unit.
    /// Null if no scaling recommended.
    /// </summary>
    /// <example>71 kg</example>
    public string? RecommendedWeightFormatted { get; set; }
}
