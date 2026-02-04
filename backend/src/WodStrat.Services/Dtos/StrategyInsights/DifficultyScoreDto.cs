namespace WodStrat.Services.Dtos;

/// <summary>
/// Overall difficulty score with factor breakdown.
/// </summary>
public class DifficultyScoreDto
{
    /// <summary>
    /// Numeric difficulty score (1-10).
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Human-readable difficulty label.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the difficulty level.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Breakdown of contributing factors.
    /// </summary>
    public DifficultyBreakdownDto Breakdown { get; set; } = new();
}
