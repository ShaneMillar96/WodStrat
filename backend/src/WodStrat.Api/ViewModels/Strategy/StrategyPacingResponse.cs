using System.Text.Json.Serialization;

namespace WodStrat.Api.ViewModels.Strategy;

/// <summary>
/// Pacing analysis without duplicate movement context.
/// </summary>
public class StrategyPacingResponse
{
    /// <summary>
    /// Pacing recommendations for each movement.
    /// Movement names and benchmark data are in MovementContexts.
    /// </summary>
    [JsonPropertyName("movements")]
    public List<MovementPacingDetailResponse> Movements { get; set; } = new();

    /// <summary>
    /// General workout strategy and approach recommendations.
    /// </summary>
    /// <example>Focus on maintaining consistent pacing through movements.</example>
    [JsonPropertyName("overallStrategyNotes")]
    public string OverallStrategyNotes { get; set; } = string.Empty;

    /// <summary>
    /// Pacing distribution summary.
    /// </summary>
    [JsonPropertyName("distribution")]
    public PacingDistributionResponse Distribution { get; set; } = new();

    /// <summary>
    /// Whether all movements had sufficient data.
    /// </summary>
    [JsonPropertyName("isComplete")]
    public bool IsComplete { get; set; }
}

/// <summary>
/// Pacing detail for a single movement (without duplicate context).
/// </summary>
public class MovementPacingDetailResponse
{
    /// <summary>
    /// Reference to movement (context available in MovementContexts).
    /// </summary>
    [JsonPropertyName("movementDefinitionId")]
    public int MovementDefinitionId { get; set; }

    /// <summary>
    /// The recommended pacing level for this movement.
    /// </summary>
    /// <example>Moderate</example>
    [JsonPropertyName("pacingLevel")]
    public string PacingLevel { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable pacing guidance text.
    /// </summary>
    /// <example>Break into sets of 7-7-7. Rest 5-10 seconds between sets.</example>
    [JsonPropertyName("guidanceText")]
    public string GuidanceText { get; set; } = string.Empty;

    /// <summary>
    /// Suggested rep breakdown for the movement.
    /// </summary>
    /// <example>[7, 7, 7]</example>
    [JsonPropertyName("recommendedSets")]
    public int[]? RecommendedSets { get; set; }
}

/// <summary>
/// Distribution summary of pacing levels within a workout.
/// </summary>
public class PacingDistributionResponse
{
    /// <summary>
    /// Count of movements with Heavy pacing.
    /// </summary>
    [JsonPropertyName("heavyCount")]
    public int HeavyCount { get; set; }

    /// <summary>
    /// Count of movements with Moderate pacing.
    /// </summary>
    [JsonPropertyName("moderateCount")]
    public int ModerateCount { get; set; }

    /// <summary>
    /// Count of movements with Light pacing.
    /// </summary>
    [JsonPropertyName("lightCount")]
    public int LightCount { get; set; }

    /// <summary>
    /// Total movements analyzed.
    /// </summary>
    [JsonPropertyName("totalMovements")]
    public int TotalMovements { get; set; }

    /// <summary>
    /// Count of movements with incomplete data.
    /// </summary>
    [JsonPropertyName("incompleteDataCount")]
    public int IncompleteDataCount { get; set; }
}
