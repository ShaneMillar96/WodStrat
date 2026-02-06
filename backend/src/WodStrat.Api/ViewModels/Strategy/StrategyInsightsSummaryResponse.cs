using System.Text.Json.Serialization;

namespace WodStrat.Api.ViewModels.Strategy;

/// <summary>
/// Strategy insights without duplicate workout context.
/// </summary>
public class StrategyInsightsSummaryResponse
{
    /// <summary>
    /// Overall difficulty assessment.
    /// </summary>
    [JsonPropertyName("difficultyScore")]
    public DifficultyScoreDetailResponse DifficultyScore { get; set; } = new();

    /// <summary>
    /// Confidence assessment for the strategy recommendations.
    /// </summary>
    [JsonPropertyName("strategyConfidence")]
    public StrategyConfidenceDetailResponse StrategyConfidence { get; set; } = new();

    /// <summary>
    /// Key movements that require special attention.
    /// References movements by ID (context in MovementContexts).
    /// </summary>
    [JsonPropertyName("keyFocusMovements")]
    public List<KeyFocusMovementDetailResponse> KeyFocusMovements { get; set; } = new();

    /// <summary>
    /// Risk alerts and recommendations.
    /// References movements by ID (context in MovementContexts).
    /// </summary>
    [JsonPropertyName("riskAlerts")]
    public List<RiskAlertDetailResponse> RiskAlerts { get; set; } = new();

    /// <summary>
    /// Strategy summary text.
    /// </summary>
    [JsonPropertyName("strategySummary")]
    public string StrategySummary { get; set; } = string.Empty;
}

/// <summary>
/// Difficulty score detail.
/// </summary>
public class DifficultyScoreDetailResponse
{
    /// <summary>
    /// Numeric difficulty score (1-10).
    /// </summary>
    [JsonPropertyName("score")]
    public int Score { get; set; }

    /// <summary>
    /// Human-readable difficulty label.
    /// </summary>
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the difficulty level.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Breakdown of contributing factors.
    /// </summary>
    [JsonPropertyName("breakdown")]
    public DifficultyBreakdownDetailResponse Breakdown { get; set; } = new();
}

/// <summary>
/// Difficulty breakdown.
/// </summary>
public class DifficultyBreakdownDetailResponse
{
    /// <summary>
    /// Pacing factor contribution (0-10 scale, weight: 40%).
    /// </summary>
    [JsonPropertyName("pacingFactor")]
    public decimal PacingFactor { get; set; }

    /// <summary>
    /// Volume factor contribution (0-10 scale, weight: 30%).
    /// </summary>
    [JsonPropertyName("volumeFactor")]
    public decimal VolumeFactor { get; set; }

    /// <summary>
    /// Time factor contribution (0-10 scale, weight: 30%).
    /// </summary>
    [JsonPropertyName("timeFactor")]
    public decimal TimeFactor { get; set; }

    /// <summary>
    /// Experience level modifier applied to base score.
    /// </summary>
    [JsonPropertyName("experienceModifier")]
    public decimal ExperienceModifier { get; set; }
}

/// <summary>
/// Strategy confidence.
/// </summary>
public class StrategyConfidenceDetailResponse
{
    /// <summary>
    /// Confidence level (High, Medium, Low).
    /// </summary>
    [JsonPropertyName("level")]
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Confidence percentage (0-100).
    /// </summary>
    [JsonPropertyName("percentage")]
    public int Percentage { get; set; }

    /// <summary>
    /// Explanation of confidence factors.
    /// </summary>
    [JsonPropertyName("explanation")]
    public string Explanation { get; set; } = string.Empty;

    /// <summary>
    /// List of movements without benchmark data affecting confidence.
    /// </summary>
    [JsonPropertyName("missingBenchmarks")]
    public List<string> MissingBenchmarks { get; set; } = new();
}

/// <summary>
/// Key focus movement with ID reference.
/// </summary>
public class KeyFocusMovementDetailResponse
{
    /// <summary>
    /// Reference to movement (context available in MovementContexts).
    /// </summary>
    [JsonPropertyName("movementDefinitionId")]
    public int MovementDefinitionId { get; set; }

    /// <summary>
    /// Reason why this movement requires special focus.
    /// </summary>
    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Strategic recommendation for approaching this movement.
    /// </summary>
    [JsonPropertyName("recommendation")]
    public string Recommendation { get; set; } = string.Empty;

    /// <summary>
    /// Priority ranking (1 = highest priority).
    /// </summary>
    [JsonPropertyName("priority")]
    public int Priority { get; set; }
}

/// <summary>
/// Risk alert with movement ID references.
/// </summary>
public class RiskAlertDetailResponse
{
    /// <summary>
    /// Type of risk alert.
    /// </summary>
    [JsonPropertyName("alertType")]
    public string AlertType { get; set; } = string.Empty;

    /// <summary>
    /// Severity level of the alert.
    /// </summary>
    [JsonPropertyName("severity")]
    public string Severity { get; set; } = string.Empty;

    /// <summary>
    /// Short title for the alert.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed message explaining the risk.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Movement IDs affected by this alert (context in MovementContexts).
    /// </summary>
    [JsonPropertyName("affectedMovementIds")]
    public List<int> AffectedMovementIds { get; set; } = new();

    /// <summary>
    /// Recommended action to address this risk.
    /// </summary>
    [JsonPropertyName("suggestedAction")]
    public string SuggestedAction { get; set; } = string.Empty;
}
