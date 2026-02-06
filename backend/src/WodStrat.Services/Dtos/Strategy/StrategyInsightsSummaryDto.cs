namespace WodStrat.Services.Dtos;

/// <summary>
/// Strategy insights for unified response.
/// </summary>
public class StrategyInsightsSummaryDto
{
    /// <summary>
    /// Difficulty score.
    /// </summary>
    public DifficultyScoreDto DifficultyScore { get; set; } = new();

    /// <summary>
    /// Strategy confidence.
    /// </summary>
    public StrategyConfidenceDto StrategyConfidence { get; set; } = new();

    /// <summary>
    /// Key focus movements (references by ID).
    /// </summary>
    public IReadOnlyList<KeyFocusMovementSummaryDto> KeyFocusMovements { get; set; } = Array.Empty<KeyFocusMovementSummaryDto>();

    /// <summary>
    /// Risk alerts.
    /// </summary>
    public IReadOnlyList<RiskAlertDto> RiskAlerts { get; set; } = Array.Empty<RiskAlertDto>();

    /// <summary>
    /// Strategy summary text.
    /// </summary>
    public string StrategySummary { get; set; } = string.Empty;
}
