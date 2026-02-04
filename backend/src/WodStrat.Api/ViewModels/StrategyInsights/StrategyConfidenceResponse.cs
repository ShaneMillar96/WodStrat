namespace WodStrat.Api.ViewModels.StrategyInsights;

/// <summary>
/// Response model for strategy confidence assessment.
/// </summary>
public class StrategyConfidenceResponse
{
    /// <summary>
    /// Confidence level (High, Medium, Low).
    /// </summary>
    /// <example>High</example>
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Confidence percentage (0-100).
    /// </summary>
    /// <example>85</example>
    public int Percentage { get; set; }

    /// <summary>
    /// Explanation of the confidence assessment.
    /// </summary>
    /// <example>Based on 85% benchmark coverage for workout movements</example>
    public string Explanation { get; set; } = string.Empty;

    /// <summary>
    /// List of benchmarks that are missing and would improve confidence if recorded.
    /// </summary>
    public List<string> MissingBenchmarks { get; set; } = new();
}
