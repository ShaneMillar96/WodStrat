namespace WodStrat.Api.ViewModels.Benchmarks;

/// <summary>
/// Response model for benchmark definition data.
/// </summary>
public class BenchmarkDefinitionResponse
{
    /// <summary>
    /// Unique identifier for the benchmark definition.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// Display name of the benchmark.
    /// </summary>
    /// <example>Fran</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL-friendly identifier for the benchmark.
    /// </summary>
    /// <example>fran</example>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the benchmark.
    /// </summary>
    /// <example>21-15-9 Thrusters (95/65 lb) and Pull-ups</example>
    public string? Description { get; set; }

    /// <summary>
    /// Benchmark category (Cardio, Strength, Gymnastics, HeroWod).
    /// </summary>
    /// <example>HeroWod</example>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Type of metric used (Time, Reps, Weight, Pace).
    /// </summary>
    /// <example>Time</example>
    public string MetricType { get; set; } = string.Empty;

    /// <summary>
    /// Display unit for the value.
    /// </summary>
    /// <example>seconds</example>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Sort order for display within category.
    /// </summary>
    /// <example>1</example>
    public int DisplayOrder { get; set; }
}
