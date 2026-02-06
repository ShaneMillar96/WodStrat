using System.Text.Json.Serialization;
using WodStrat.Api.ViewModels.Pacing;

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
    /// For cardio movements, this contains pace-based guidance instead of set-based guidance.
    /// </summary>
    /// <example>Break into sets of 7-7-7. Rest 5-10 seconds between sets.</example>
    [JsonPropertyName("guidanceText")]
    public string GuidanceText { get; set; } = string.Empty;

    /// <summary>
    /// Suggested rep breakdown for the movement.
    /// Empty/null for cardio movements where pacing is pace-based.
    /// </summary>
    /// <example>[7, 7, 7]</example>
    [JsonPropertyName("recommendedSets")]
    public int[]? RecommendedSets { get; set; }

    /// <summary>
    /// Whether this movement is a cardio/monostructural movement (e.g., Run, Row, Bike).
    /// When true, pacing guidance is pace-based rather than set-based.
    /// </summary>
    /// <example>true</example>
    [JsonPropertyName("isCardio")]
    public bool IsCardio { get; set; }

    /// <summary>
    /// Target pace information for cardio movements.
    /// Null for non-cardio movements (strength, gymnastics).
    /// </summary>
    [JsonPropertyName("targetPace")]
    public CardioPaceResponse? TargetPace { get; set; }
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
