namespace WodStrat.Services.Dtos;

/// <summary>
/// Data transfer object for athlete benchmark responses.
/// Includes formatted value based on metric type.
/// </summary>
public class AthleteBenchmarkDto
{
    /// <summary>
    /// Unique identifier for the athlete benchmark.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Reference to the athlete who recorded this benchmark.
    /// </summary>
    public int AthleteId { get; set; }

    /// <summary>
    /// Reference to the benchmark definition.
    /// </summary>
    public int BenchmarkDefinitionId { get; set; }

    /// <summary>
    /// Display name of the benchmark (denormalized for convenience).
    /// </summary>
    public string BenchmarkName { get; set; } = string.Empty;

    /// <summary>
    /// URL-friendly slug of the benchmark (denormalized for convenience).
    /// </summary>
    public string BenchmarkSlug { get; set; } = string.Empty;

    /// <summary>
    /// Category of the benchmark (denormalized for convenience).
    /// </summary>
    public string BenchmarkCategory { get; set; } = string.Empty;

    /// <summary>
    /// Metric type for value interpretation (denormalized for convenience).
    /// </summary>
    public string MetricType { get; set; } = string.Empty;

    /// <summary>
    /// Display unit for the value (denormalized for convenience).
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// The raw benchmark value (interpretation depends on MetricType).
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Human-readable formatted value (e.g., "3:45" for time, "100 kg" for weight).
    /// </summary>
    public string FormattedValue { get; set; } = string.Empty;

    /// <summary>
    /// Date when the benchmark result was recorded.
    /// </summary>
    public DateOnly RecordedAt { get; set; }

    /// <summary>
    /// Optional notes about this benchmark result.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Timestamp when the record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the record was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
