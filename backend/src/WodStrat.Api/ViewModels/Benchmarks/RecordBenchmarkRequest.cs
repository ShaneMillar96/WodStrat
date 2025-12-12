namespace WodStrat.Api.ViewModels.Benchmarks;

/// <summary>
/// Request model for recording a new benchmark result.
/// </summary>
public class RecordBenchmarkRequest
{
    /// <summary>
    /// The benchmark definition to record a result for.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid BenchmarkDefinitionId { get; set; }

    /// <summary>
    /// The benchmark value. Interpretation depends on the benchmark's metric type:
    /// - Time: total seconds (e.g., 195.5 for 3:15.5)
    /// - Reps: count (e.g., 50)
    /// - Weight: kilograms (e.g., 100.5)
    /// - Pace: seconds per unit (e.g., 105 for 1:45/500m)
    /// </summary>
    /// <example>195.5</example>
    public decimal Value { get; set; }

    /// <summary>
    /// When the benchmark was achieved. Defaults to today if not provided.
    /// </summary>
    /// <example>2024-01-15</example>
    public DateOnly? RecordedAt { get; set; }

    /// <summary>
    /// Optional notes about the benchmark result (e.g., "RX", "scaled", equipment used).
    /// </summary>
    /// <example>RX</example>
    public string? Notes { get; set; }
}
