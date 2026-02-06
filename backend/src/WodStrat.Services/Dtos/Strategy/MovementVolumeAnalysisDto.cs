namespace WodStrat.Services.Dtos;

/// <summary>
/// Volume load analysis for a movement, referencing shared context by ID.
/// This is a slim version that avoids duplicating movement data.
/// </summary>
public class MovementVolumeAnalysisDto
{
    /// <summary>
    /// Reference to movement in the shared context.
    /// </summary>
    public int MovementDefinitionId { get; set; }

    /// <summary>
    /// Weight per repetition.
    /// </summary>
    public decimal Weight { get; set; }

    /// <summary>
    /// Unit of weight measurement.
    /// </summary>
    public string WeightUnit { get; set; } = "kg";

    /// <summary>
    /// Number of repetitions per round.
    /// </summary>
    public int Reps { get; set; }

    /// <summary>
    /// Number of rounds.
    /// </summary>
    public int Rounds { get; set; }

    /// <summary>
    /// Calculated total volume load.
    /// </summary>
    public decimal VolumeLoad { get; set; }

    /// <summary>
    /// Formatted volume load string.
    /// </summary>
    public string VolumeLoadFormatted { get; set; } = string.Empty;

    /// <summary>
    /// Load intensity classification.
    /// </summary>
    public string LoadClassification { get; set; } = string.Empty;

    /// <summary>
    /// Actionable recommendation for this movement.
    /// </summary>
    public string Tip { get; set; } = string.Empty;

    /// <summary>
    /// Suggested scaled weight (null if RX is appropriate).
    /// </summary>
    public decimal? RecommendedWeight { get; set; }

    /// <summary>
    /// Formatted recommended weight string.
    /// </summary>
    public string? RecommendedWeightFormatted { get; set; }
}
