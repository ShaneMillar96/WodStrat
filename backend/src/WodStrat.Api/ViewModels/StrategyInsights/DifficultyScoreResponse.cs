namespace WodStrat.Api.ViewModels.StrategyInsights;

/// <summary>
/// Response model for workout difficulty assessment.
/// </summary>
public class DifficultyScoreResponse
{
    /// <summary>
    /// Overall difficulty score on a 1-10 scale.
    /// </summary>
    /// <example>7</example>
    public int Score { get; set; }

    /// <summary>
    /// Human-readable difficulty label.
    /// </summary>
    /// <example>Challenging</example>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the difficulty assessment.
    /// </summary>
    /// <example>This workout is challenging for your current fitness level</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Breakdown of factors contributing to the difficulty score.
    /// </summary>
    public DifficultyBreakdownResponse Breakdown { get; set; } = new();
}
