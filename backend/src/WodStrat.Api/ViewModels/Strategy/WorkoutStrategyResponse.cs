using System.Text.Json.Serialization;

namespace WodStrat.Api.ViewModels.Strategy;

/// <summary>
/// Comprehensive workout strategy response combining all analysis data.
/// </summary>
public class WorkoutStrategyResponse
{
    /// <summary>
    /// Shared workout context (eliminates duplicate workoutId/workoutName across responses).
    /// </summary>
    [JsonPropertyName("workout")]
    public WorkoutContextResponse Workout { get; set; } = new();

    /// <summary>
    /// Shared movement context for all movements in the workout.
    /// Keyed by movementDefinitionId, provides benchmark data once per movement.
    /// </summary>
    [JsonPropertyName("movementContexts")]
    public List<MovementContextResponse> MovementContexts { get; set; } = new();

    /// <summary>
    /// Pacing analysis for the workout.
    /// </summary>
    [JsonPropertyName("pacing")]
    public StrategyPacingResponse? Pacing { get; set; }

    /// <summary>
    /// Volume load analysis for the workout (null if no weighted movements).
    /// </summary>
    [JsonPropertyName("volumeLoad")]
    public StrategyVolumeLoadResponse? VolumeLoad { get; set; }

    /// <summary>
    /// Time estimate analysis for the workout.
    /// </summary>
    [JsonPropertyName("timeEstimate")]
    public StrategyTimeEstimateResponse? TimeEstimate { get; set; }

    /// <summary>
    /// Strategy insights including difficulty, confidence, focus movements, and alerts.
    /// </summary>
    [JsonPropertyName("insights")]
    public StrategyInsightsSummaryResponse? Insights { get; set; }

    /// <summary>
    /// Timestamp when the strategy was calculated.
    /// </summary>
    /// <example>2024-01-15T10:30:00Z</example>
    [JsonPropertyName("calculatedAt")]
    public DateTime CalculatedAt { get; set; }
}
