namespace WodStrat.Dal.Models;

/// <summary>
/// Represents an athlete's benchmark result linking athletes to their performance metrics.
/// </summary>
public class AthleteBenchmark : EntityBase
{
    /// <summary>
    /// Reference to the athlete who recorded this benchmark.
    /// </summary>
    public Guid AthleteId { get; set; }

    /// <summary>
    /// Reference to the benchmark type.
    /// </summary>
    public Guid BenchmarkDefinitionId { get; set; }

    /// <summary>
    /// The benchmark result value (interpretation depends on metric_type).
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Date when this benchmark was achieved.
    /// </summary>
    public DateOnly RecordedAt { get; set; }

    /// <summary>
    /// Optional notes (e.g., "RX", "scaled", equipment used).
    /// </summary>
    public string? Notes { get; set; }

    // Navigation properties
    /// <summary>
    /// The athlete who recorded this benchmark.
    /// </summary>
    public Athlete Athlete { get; set; } = null!;

    /// <summary>
    /// The benchmark definition for this result.
    /// </summary>
    public BenchmarkDefinition BenchmarkDefinition { get; set; } = null!;
}
