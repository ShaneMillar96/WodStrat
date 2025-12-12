namespace WodStrat.Services.Dtos;

/// <summary>
/// Data transfer object for updating an existing athlete benchmark result.
/// </summary>
public class UpdateBenchmarkDto
{
    /// <summary>
    /// The updated benchmark value. Interpretation depends on metric type:
    /// - Time: total seconds (e.g., 225 for 3:45)
    /// - Reps: count (e.g., 50)
    /// - Weight: kilograms (e.g., 100.5)
    /// - Pace: seconds per unit (e.g., 105 for 1:45/500m)
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Updated date when the benchmark was recorded.
    /// </summary>
    public DateOnly? RecordedAt { get; set; }

    /// <summary>
    /// Updated notes about this benchmark result. Max 500 characters.
    /// </summary>
    public string? Notes { get; set; }
}
