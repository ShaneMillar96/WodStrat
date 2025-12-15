namespace WodStrat.Api.ViewModels.Benchmarks;

/// <summary>
/// Response model for athlete benchmark result with formatted display value.
/// </summary>
public class AthleteBenchmarkResponse
{
    /// <summary>
    /// Unique identifier for the athlete benchmark.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// Reference to the athlete who recorded this benchmark.
    /// </summary>
    /// <example>1</example>
    public int AthleteId { get; set; }

    /// <summary>
    /// Reference to the benchmark definition.
    /// </summary>
    /// <example>5</example>
    public int BenchmarkDefinitionId { get; set; }

    /// <summary>
    /// Display name of the benchmark (denormalized for convenience).
    /// </summary>
    /// <example>Fran</example>
    public string BenchmarkName { get; set; } = string.Empty;

    /// <summary>
    /// Category of the benchmark (denormalized for convenience).
    /// </summary>
    /// <example>HeroWod</example>
    public string BenchmarkCategory { get; set; } = string.Empty;

    /// <summary>
    /// The raw benchmark value (interpretation depends on metric type).
    /// </summary>
    /// <example>195.5</example>
    public decimal Value { get; set; }

    /// <summary>
    /// Human-readable formatted value (e.g., "3:15" for time, "100 kg" for weight).
    /// </summary>
    /// <example>3:15</example>
    public string FormattedValue { get; set; } = string.Empty;

    /// <summary>
    /// Display unit for the value.
    /// </summary>
    /// <example>seconds</example>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// When the benchmark result was recorded.
    /// </summary>
    /// <example>2024-01-15</example>
    public DateOnly RecordedAt { get; set; }

    /// <summary>
    /// Optional notes about this benchmark result.
    /// </summary>
    /// <example>RX</example>
    public string? Notes { get; set; }

    /// <summary>
    /// Timestamp when the record was created.
    /// </summary>
    /// <example>2024-01-15T10:30:00Z</example>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the record was last updated.
    /// </summary>
    /// <example>2024-01-15T10:30:00Z</example>
    public DateTime UpdatedAt { get; set; }
}
