namespace WodStrat.Api.ViewModels.StrategyInsights;

/// <summary>
/// Response model for difficulty score breakdown factors.
/// </summary>
public class DifficultyBreakdownResponse
{
    /// <summary>
    /// Difficulty contribution from pacing analysis (0-10 scale).
    /// </summary>
    /// <example>6.5</example>
    public decimal PacingFactor { get; set; }

    /// <summary>
    /// Difficulty contribution from volume load analysis (0-10 scale).
    /// </summary>
    /// <example>5.0</example>
    public decimal VolumeFactor { get; set; }

    /// <summary>
    /// Difficulty contribution from time estimate analysis (0-10 scale).
    /// </summary>
    /// <example>4.5</example>
    public decimal TimeFactor { get; set; }

    /// <summary>
    /// Modifier applied based on athlete's experience level.
    /// </summary>
    /// <example>1.0</example>
    public decimal ExperienceModifier { get; set; }
}
