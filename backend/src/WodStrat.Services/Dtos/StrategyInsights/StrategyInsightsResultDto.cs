namespace WodStrat.Services.Dtos;

/// <summary>
/// Complete strategy insights result for a workout.
/// </summary>
public class StrategyInsightsResultDto
{
    /// <summary>
    /// Reference to the workout.
    /// </summary>
    public int WorkoutId { get; set; }

    /// <summary>
    /// Display name of the workout.
    /// </summary>
    public string WorkoutName { get; set; } = string.Empty;

    /// <summary>
    /// Type of workout (AMRAP, ForTime, etc.).
    /// </summary>
    public string WorkoutType { get; set; } = string.Empty;

    /// <summary>
    /// Overall difficulty assessment (1-10 scale).
    /// </summary>
    public DifficultyScoreDto DifficultyScore { get; set; } = new();

    /// <summary>
    /// Confidence level of the strategy recommendations.
    /// </summary>
    public StrategyConfidenceDto StrategyConfidence { get; set; } = new();

    /// <summary>
    /// Top 1-3 movements requiring most attention.
    /// </summary>
    public IReadOnlyList<KeyFocusMovementDto> KeyFocusMovements { get; set; } = Array.Empty<KeyFocusMovementDto>();

    /// <summary>
    /// Risk alerts and warnings based on combined analysis.
    /// </summary>
    public IReadOnlyList<RiskAlertDto> RiskAlerts { get; set; } = Array.Empty<RiskAlertDto>();

    /// <summary>
    /// Summary strategy recommendation text.
    /// </summary>
    public string StrategySummary { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the insights were calculated.
    /// </summary>
    public DateTime CalculatedAt { get; set; }

    /// <summary>
    /// Reference to the underlying pacing analysis (for detailed drill-down).
    /// </summary>
    public WorkoutPacingResultDto? PacingAnalysis { get; set; }

    /// <summary>
    /// Reference to the underlying volume load analysis (for detailed drill-down).
    /// </summary>
    public WorkoutVolumeLoadResultDto? VolumeLoadAnalysis { get; set; }

    /// <summary>
    /// Reference to the underlying time estimate (for detailed drill-down).
    /// </summary>
    public TimeEstimateResultDto? TimeEstimate { get; set; }
}
