namespace WodStrat.Api.ViewModels.StrategyInsights;

/// <summary>
/// Response model for comprehensive workout strategy insights.
/// </summary>
public class StrategyInsightsResponse
{
    /// <summary>
    /// The workout's unique identifier.
    /// </summary>
    /// <example>123</example>
    public int WorkoutId { get; set; }

    /// <summary>
    /// Display name of the workout.
    /// </summary>
    /// <example>Fran</example>
    public string WorkoutName { get; set; } = string.Empty;

    /// <summary>
    /// Overall difficulty assessment for the workout relative to athlete's fitness level.
    /// </summary>
    public DifficultyScoreResponse DifficultyScore { get; set; } = new();

    /// <summary>
    /// Confidence assessment for the strategy recommendations.
    /// </summary>
    public StrategyConfidenceResponse StrategyConfidence { get; set; } = new();

    /// <summary>
    /// Key movements that require special attention during the workout.
    /// </summary>
    public List<KeyFocusMovementResponse> KeyFocusMovements { get; set; } = new();

    /// <summary>
    /// Risk alerts and recommendations for the workout.
    /// </summary>
    public List<RiskAlertResponse> RiskAlerts { get; set; } = new();

    /// <summary>
    /// Timestamp when the insights were calculated.
    /// </summary>
    /// <example>2024-01-15T10:30:00Z</example>
    public DateTime CalculatedAt { get; set; }
}
