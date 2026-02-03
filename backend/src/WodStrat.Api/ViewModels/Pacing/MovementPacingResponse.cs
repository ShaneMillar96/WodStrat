namespace WodStrat.Api.ViewModels.Pacing;

/// <summary>
/// Response model for individual movement pacing recommendation.
/// </summary>
public class MovementPacingResponse
{
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
    /// The recommended pacing level for this movement.
    /// </summary>
    /// <example>Moderate</example>
    public string PacingLevel { get; set; } = string.Empty;

    /// <summary>
    /// The athlete's percentile ranking for this movement type (0.0 to 1.0).
    /// Higher values indicate stronger relative performance.
    /// </summary>
    /// <example>0.65</example>
    public decimal AthletePercentile { get; set; }

    /// <summary>
    /// Human-readable pacing guidance text.
    /// </summary>
    /// <example>Break into sets of 7-7-7. Rest 5-10 seconds between sets.</example>
    public string GuidanceText { get; set; } = string.Empty;

    /// <summary>
    /// Suggested rep breakdown for the movement.
    /// </summary>
    /// <example>[7, 7, 7]</example>
    public int[]? RecommendedSets { get; set; }

    /// <summary>
    /// The benchmark that was used to determine this pacing recommendation.
    /// </summary>
    /// <example>Max Unbroken Pull-Ups</example>
    public string? BenchmarkUsed { get; set; }
}
