using System.Text.Json.Serialization;

namespace WodStrat.Api.ViewModels.Strategy;

/// <summary>
/// Time estimate analysis without duplicate workout context.
/// </summary>
public class StrategyTimeEstimateResponse
{
    /// <summary>
    /// Type of estimate provided (Time or RoundsReps).
    /// </summary>
    /// <example>Time</example>
    [JsonPropertyName("estimateType")]
    public string EstimateType { get; set; } = string.Empty;

    /// <summary>
    /// Minimum estimated value.
    /// </summary>
    [JsonPropertyName("minEstimate")]
    public int MinEstimate { get; set; }

    /// <summary>
    /// Maximum estimated value.
    /// </summary>
    [JsonPropertyName("maxEstimate")]
    public int MaxEstimate { get; set; }

    /// <summary>
    /// Additional reps beyond complete rounds (AMRAP only).
    /// </summary>
    [JsonPropertyName("minExtraReps")]
    public int? MinExtraReps { get; set; }

    /// <summary>
    /// Additional reps beyond complete rounds (AMRAP only).
    /// </summary>
    [JsonPropertyName("maxExtraReps")]
    public int? MaxExtraReps { get; set; }

    /// <summary>
    /// Human-readable formatted estimate range.
    /// </summary>
    /// <example>3:00 - 4:00</example>
    [JsonPropertyName("formattedRange")]
    public string FormattedRange { get; set; } = string.Empty;

    /// <summary>
    /// Confidence level in the estimate.
    /// </summary>
    /// <example>High</example>
    [JsonPropertyName("confidenceLevel")]
    public string ConfidenceLevel { get; set; } = string.Empty;

    /// <summary>
    /// Summary of factors that influenced the estimate.
    /// </summary>
    [JsonPropertyName("factorsSummary")]
    public string FactorsSummary { get; set; } = string.Empty;

    /// <summary>
    /// Recommended rest periods during the workout.
    /// References movements by ID (context in MovementContexts).
    /// </summary>
    [JsonPropertyName("restRecommendations")]
    public List<RestRecommendationDetailResponse> RestRecommendations { get; set; } = new();

    /// <summary>
    /// EMOM-specific feasibility analysis (null for non-EMOM workouts).
    /// </summary>
    [JsonPropertyName("emomFeasibility")]
    public List<EmomMinuteDetailResponse>? EmomFeasibility { get; set; }

    /// <summary>
    /// Number of movements with benchmark coverage.
    /// </summary>
    [JsonPropertyName("benchmarkCoverageCount")]
    public int BenchmarkCoverageCount { get; set; }

    /// <summary>
    /// Total movements in the workout.
    /// </summary>
    [JsonPropertyName("totalMovementCount")]
    public int TotalMovementCount { get; set; }

    /// <summary>
    /// Average athlete percentile across covered movements.
    /// </summary>
    [JsonPropertyName("averagePercentile")]
    public decimal AveragePercentile { get; set; }
}

/// <summary>
/// Rest recommendation with movement reference by ID.
/// </summary>
public class RestRecommendationDetailResponse
{
    /// <summary>
    /// Reference to movement (context available in MovementContexts).
    /// </summary>
    [JsonPropertyName("movementDefinitionId")]
    public int MovementDefinitionId { get; set; }

    /// <summary>
    /// Suggested rest duration in seconds.
    /// </summary>
    [JsonPropertyName("suggestedRestSeconds")]
    public int SuggestedRestSeconds { get; set; }

    /// <summary>
    /// Human-readable rest range.
    /// </summary>
    /// <example>8-12 seconds</example>
    [JsonPropertyName("restRange")]
    public string RestRange { get; set; } = string.Empty;

    /// <summary>
    /// Explanation for why rest is recommended at this point.
    /// </summary>
    [JsonPropertyName("reasoning")]
    public string Reasoning { get; set; } = string.Empty;

    /// <summary>
    /// The pacing level for this movement (Light/Moderate/Heavy).
    /// </summary>
    [JsonPropertyName("pacingLevel")]
    public string PacingLevel { get; set; } = string.Empty;
}

/// <summary>
/// EMOM minute feasibility with movement references by ID.
/// </summary>
public class EmomMinuteDetailResponse
{
    /// <summary>
    /// The minute number (1-based).
    /// </summary>
    [JsonPropertyName("minute")]
    public int Minute { get; set; }

    /// <summary>
    /// Description of the prescribed work for this minute.
    /// </summary>
    /// <example>10 Power Cleans @ 60kg</example>
    [JsonPropertyName("prescribedWork")]
    public string PrescribedWork { get; set; } = string.Empty;

    /// <summary>
    /// Estimated time to complete the prescribed work in seconds.
    /// </summary>
    [JsonPropertyName("estimatedCompletionSeconds")]
    public int EstimatedCompletionSeconds { get; set; }

    /// <summary>
    /// Whether the prescribed work can be completed within 60 seconds.
    /// </summary>
    [JsonPropertyName("isFeasible")]
    public bool IsFeasible { get; set; }

    /// <summary>
    /// Remaining seconds after completing the prescribed work.
    /// </summary>
    [JsonPropertyName("bufferSeconds")]
    public int BufferSeconds { get; set; }

    /// <summary>
    /// Guidance for approaching this minute.
    /// </summary>
    [JsonPropertyName("recommendation")]
    public string Recommendation { get; set; } = string.Empty;

    /// <summary>
    /// Movement IDs involved in this minute (context in MovementContexts).
    /// </summary>
    [JsonPropertyName("movementDefinitionIds")]
    public IReadOnlyList<int> MovementDefinitionIds { get; set; } = Array.Empty<int>();
}
