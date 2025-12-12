namespace WodStrat.Services.Dtos;

/// <summary>
/// Data transfer object for recording a new athlete benchmark result.
/// </summary>
public class RecordBenchmarkDto
{
    /// <summary>
    /// Reference to the benchmark definition. Required.
    /// </summary>
    public Guid BenchmarkDefinitionId { get; set; }

    /// <summary>
    /// The benchmark value. Interpretation depends on metric type:
    /// - Time: total seconds (e.g., 225 for 3:45)
    /// - Reps: count (e.g., 50)
    /// - Weight: kilograms (e.g., 100.5)
    /// - Pace: seconds per unit (e.g., 105 for 1:45/500m)
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Date when the benchmark was recorded. Defaults to today if not provided.
    /// </summary>
    public DateOnly? RecordedAt { get; set; }

    /// <summary>
    /// Optional notes about this benchmark result. Max 500 characters.
    /// </summary>
    public string? Notes { get; set; }
}
