namespace WodStrat.Api.ViewModels.Benchmarks;

/// <summary>
/// Request model for updating an existing benchmark result.
/// </summary>
public class UpdateBenchmarkRequest
{
    /// <summary>
    /// The updated benchmark value.
    /// </summary>
    /// <example>185.0</example>
    public decimal Value { get; set; }

    /// <summary>
    /// Updated date when the benchmark was achieved.
    /// </summary>
    /// <example>2024-01-20</example>
    public DateOnly? RecordedAt { get; set; }

    /// <summary>
    /// Updated notes about the benchmark result.
    /// </summary>
    /// <example>PR - RX</example>
    public string? Notes { get; set; }
}
