namespace WodStrat.Services.Dtos;

/// <summary>
/// Data transfer object for benchmark definition responses.
/// </summary>
public class BenchmarkDefinitionDto
{
    /// <summary>
    /// Unique identifier for the benchmark definition.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Display name of the benchmark (e.g., "500m Row").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL-friendly slug for the benchmark (e.g., "500m-row").
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the benchmark.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Benchmark category (Cardio, Strength, Gymnastics, HeroWod).
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Type of metric stored (Time, Reps, Weight, Pace).
    /// </summary>
    public string MetricType { get; set; } = string.Empty;

    /// <summary>
    /// Display unit for the value (e.g., "seconds", "kg", "reps").
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Sort order for display within category.
    /// </summary>
    public int DisplayOrder { get; set; }
}
