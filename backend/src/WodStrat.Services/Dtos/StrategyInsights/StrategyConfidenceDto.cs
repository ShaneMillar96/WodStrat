namespace WodStrat.Services.Dtos;

/// <summary>
/// Confidence level of strategy recommendations.
/// </summary>
public class StrategyConfidenceDto
{
    /// <summary>
    /// Confidence level (High, Medium, Low).
    /// </summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Confidence percentage (0-100).
    /// </summary>
    public int Percentage { get; set; }

    /// <summary>
    /// Explanation of confidence factors.
    /// </summary>
    public string Explanation { get; set; } = string.Empty;

    /// <summary>
    /// List of movements without benchmark data affecting confidence.
    /// </summary>
    public IReadOnlyList<string> MissingBenchmarks { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Number of movements with complete benchmark coverage.
    /// </summary>
    public int CoveredMovementCount { get; set; }

    /// <summary>
    /// Total number of movements in the workout.
    /// </summary>
    public int TotalMovementCount { get; set; }
}
