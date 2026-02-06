using System.Text.Json.Serialization;

namespace WodStrat.Api.ViewModels.Strategy;

/// <summary>
/// Shared context for a movement, eliminating duplicate data across strategy responses.
/// </summary>
public class MovementContextResponse
{
    /// <summary>
    /// Reference to the movement definition.
    /// </summary>
    /// <example>15</example>
    [JsonPropertyName("movementDefinitionId")]
    public int MovementDefinitionId { get; set; }

    /// <summary>
    /// Display name of the movement.
    /// </summary>
    /// <example>Pull-Up</example>
    [JsonPropertyName("movementName")]
    public string MovementName { get; set; } = string.Empty;

    /// <summary>
    /// The athlete's percentile ranking for this movement type (0.0 to 1.0).
    /// Null if no benchmark data available.
    /// </summary>
    /// <example>0.65</example>
    [JsonPropertyName("athletePercentile")]
    public decimal? AthletePercentile { get; set; }

    /// <summary>
    /// The benchmark that was used to determine calculations for this movement.
    /// </summary>
    /// <example>Max Unbroken Pull-Ups</example>
    [JsonPropertyName("benchmarkUsed")]
    public string? BenchmarkUsed { get; set; }

    /// <summary>
    /// Whether the athlete has benchmark data for this movement.
    /// </summary>
    /// <example>true</example>
    [JsonPropertyName("hasBenchmarkData")]
    public bool HasBenchmarkData { get; set; }
}
