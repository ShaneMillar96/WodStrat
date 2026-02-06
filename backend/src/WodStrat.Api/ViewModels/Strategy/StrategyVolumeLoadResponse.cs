using System.Text.Json.Serialization;

namespace WodStrat.Api.ViewModels.Strategy;

/// <summary>
/// Volume load analysis without duplicate movement context.
/// </summary>
public class StrategyVolumeLoadResponse
{
    /// <summary>
    /// Volume load analysis for each weighted movement.
    /// </summary>
    [JsonPropertyName("movements")]
    public List<MovementVolumeLoadDetailResponse> Movements { get; set; } = new();

    /// <summary>
    /// Total volume load across all movements (sum of weight x reps x rounds).
    /// </summary>
    /// <example>5430.00</example>
    [JsonPropertyName("totalVolumeLoad")]
    public decimal TotalVolumeLoad { get; set; }

    /// <summary>
    /// Human-readable formatted total volume load.
    /// </summary>
    /// <example>5,430 kg</example>
    [JsonPropertyName("totalVolumeLoadFormatted")]
    public string TotalVolumeLoadFormatted { get; set; } = string.Empty;

    /// <summary>
    /// Overall assessment summary of the workout's volume load.
    /// </summary>
    [JsonPropertyName("overallAssessment")]
    public string OverallAssessment { get; set; } = string.Empty;

    /// <summary>
    /// Volume distribution summary.
    /// </summary>
    [JsonPropertyName("distribution")]
    public VolumeLoadDistributionResponse Distribution { get; set; } = new();

    /// <summary>
    /// Whether all movements had sufficient data.
    /// </summary>
    [JsonPropertyName("isComplete")]
    public bool IsComplete { get; set; }
}

/// <summary>
/// Volume load detail for a single movement (without duplicate context).
/// </summary>
public class MovementVolumeLoadDetailResponse
{
    /// <summary>
    /// Reference to movement (context available in MovementContexts).
    /// </summary>
    [JsonPropertyName("movementDefinitionId")]
    public int MovementDefinitionId { get; set; }

    /// <summary>
    /// Weight used for this movement.
    /// </summary>
    /// <example>102.5</example>
    [JsonPropertyName("weight")]
    public decimal Weight { get; set; }

    /// <summary>
    /// Unit of weight measurement.
    /// </summary>
    /// <example>kg</example>
    [JsonPropertyName("weightUnit")]
    public string WeightUnit { get; set; } = string.Empty;

    /// <summary>
    /// Number of repetitions per round.
    /// </summary>
    [JsonPropertyName("reps")]
    public int Reps { get; set; }

    /// <summary>
    /// Number of rounds in the workout.
    /// </summary>
    [JsonPropertyName("rounds")]
    public int Rounds { get; set; }

    /// <summary>
    /// Calculated volume load (weight x reps x rounds).
    /// </summary>
    [JsonPropertyName("volumeLoad")]
    public decimal VolumeLoad { get; set; }

    /// <summary>
    /// Human-readable formatted volume load.
    /// </summary>
    [JsonPropertyName("volumeLoadFormatted")]
    public string VolumeLoadFormatted { get; set; } = string.Empty;

    /// <summary>
    /// Load classification based on athlete's strength benchmarks.
    /// </summary>
    /// <example>High</example>
    [JsonPropertyName("loadClassification")]
    public string LoadClassification { get; set; } = string.Empty;

    /// <summary>
    /// Actionable tip for this movement including scaling recommendations.
    /// </summary>
    [JsonPropertyName("tip")]
    public string Tip { get; set; } = string.Empty;

    /// <summary>
    /// Recommended scaled weight if the load is classified as high.
    /// </summary>
    [JsonPropertyName("recommendedWeight")]
    public decimal? RecommendedWeight { get; set; }

    /// <summary>
    /// Human-readable recommended weight with unit.
    /// </summary>
    [JsonPropertyName("recommendedWeightFormatted")]
    public string? RecommendedWeightFormatted { get; set; }
}

/// <summary>
/// Distribution summary of load classifications within a workout.
/// </summary>
public class VolumeLoadDistributionResponse
{
    /// <summary>
    /// Count of movements with High load classification.
    /// </summary>
    [JsonPropertyName("highCount")]
    public int HighCount { get; set; }

    /// <summary>
    /// Count of movements with Moderate load classification.
    /// </summary>
    [JsonPropertyName("moderateCount")]
    public int ModerateCount { get; set; }

    /// <summary>
    /// Count of movements with Low load classification.
    /// </summary>
    [JsonPropertyName("lowCount")]
    public int LowCount { get; set; }

    /// <summary>
    /// Count of movements without external load (bodyweight).
    /// </summary>
    [JsonPropertyName("bodyweightCount")]
    public int BodyweightCount { get; set; }

    /// <summary>
    /// Total movements analyzed.
    /// </summary>
    [JsonPropertyName("totalMovements")]
    public int TotalMovements { get; set; }

    /// <summary>
    /// Count of movements with insufficient benchmark data.
    /// </summary>
    [JsonPropertyName("insufficientDataCount")]
    public int InsufficientDataCount { get; set; }
}
