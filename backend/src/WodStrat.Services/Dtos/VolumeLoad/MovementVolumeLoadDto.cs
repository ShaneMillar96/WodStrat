namespace WodStrat.Services.Dtos;

/// <summary>
/// Volume load analysis result for a single movement.
/// </summary>
public class MovementVolumeLoadDto
{
    /// <summary>
    /// Reference to the movement definition.
    /// </summary>
    public int MovementDefinitionId { get; set; }

    /// <summary>
    /// Display name of the movement.
    /// </summary>
    public string MovementName { get; set; } = string.Empty;

    /// <summary>
    /// Weight per repetition.
    /// </summary>
    public decimal Weight { get; set; }

    /// <summary>
    /// Unit of weight measurement (default: "kg").
    /// </summary>
    public string WeightUnit { get; set; } = "kg";

    /// <summary>
    /// Number of repetitions per round.
    /// </summary>
    public int Reps { get; set; }

    /// <summary>
    /// Number of rounds in the workout.
    /// </summary>
    public int Rounds { get; set; }

    /// <summary>
    /// Calculated total volume load (Weight x Reps x Rounds).
    /// </summary>
    public decimal VolumeLoad { get; set; }

    /// <summary>
    /// Formatted volume load string (e.g., "2,150 kg").
    /// </summary>
    public string VolumeLoadFormatted { get; set; } = string.Empty;

    /// <summary>
    /// Load intensity classification: "High", "Moderate", or "Low".
    /// </summary>
    public string LoadClassification { get; set; } = string.Empty;

    /// <summary>
    /// Name of the benchmark used to determine classification.
    /// </summary>
    public string BenchmarkUsed { get; set; } = string.Empty;

    /// <summary>
    /// Athlete's percentile ranking for the relevant benchmark (0-100).
    /// Null if no benchmark data available.
    /// </summary>
    public decimal? AthleteBenchmarkPercentile { get; set; }

    /// <summary>
    /// Actionable recommendation for this movement.
    /// </summary>
    public string Tip { get; set; } = string.Empty;

    /// <summary>
    /// Suggested scaled weight for the athlete.
    /// Null if RX weight is appropriate.
    /// </summary>
    public decimal? RecommendedWeight { get; set; }

    /// <summary>
    /// Formatted recommended weight string (e.g., "34 kg (80% of RX)").
    /// Null if RX weight is appropriate.
    /// </summary>
    public string? RecommendedWeightFormatted { get; set; }

    /// <summary>
    /// Whether sufficient data was available for accurate classification.
    /// </summary>
    public bool HasSufficientData { get; set; }
}
