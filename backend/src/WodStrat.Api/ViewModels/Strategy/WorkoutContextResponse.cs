using System.Text.Json.Serialization;

namespace WodStrat.Api.ViewModels.Strategy;

/// <summary>
/// Shared workout context information.
/// </summary>
public class WorkoutContextResponse
{
    /// <summary>
    /// The workout's unique identifier.
    /// </summary>
    /// <example>5</example>
    [JsonPropertyName("workoutId")]
    public int WorkoutId { get; set; }

    /// <summary>
    /// Display name of the workout.
    /// </summary>
    /// <example>Fran</example>
    [JsonPropertyName("workoutName")]
    public string WorkoutName { get; set; } = string.Empty;

    /// <summary>
    /// Type of workout (ForTime, AMRAP, EMOM, etc.).
    /// </summary>
    /// <example>ForTime</example>
    [JsonPropertyName("workoutType")]
    public string WorkoutType { get; set; } = string.Empty;

    /// <summary>
    /// Total number of rounds in the workout (if applicable).
    /// </summary>
    /// <example>5</example>
    [JsonPropertyName("rounds")]
    public int? Rounds { get; set; }

    /// <summary>
    /// Time cap in seconds (if applicable).
    /// </summary>
    /// <example>1200</example>
    [JsonPropertyName("timeCap")]
    public int? TimeCap { get; set; }
}
