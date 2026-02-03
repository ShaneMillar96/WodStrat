using WodStrat.Dal.Enums;

namespace WodStrat.Dal.Models;

/// <summary>
/// Represents a predefined benchmark type for athlete performance tracking.
/// </summary>
public class BenchmarkDefinition
{
    /// <summary>
    /// Unique auto-incrementing identifier for the benchmark definition.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Display name of the benchmark (e.g., "Fran", "Back Squat 1RM").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL-friendly identifier (e.g., "fran", "back-squat-1rm").
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the benchmark.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Benchmark category (cardio/strength/gymnastics/hero_wod).
    /// </summary>
    public BenchmarkCategory Category { get; set; }

    /// <summary>
    /// How results are measured (time/reps/weight/pace).
    /// </summary>
    public BenchmarkMetricType MetricType { get; set; }

    /// <summary>
    /// Display unit for the metric (e.g., "seconds", "kg", "reps").
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Whether this benchmark is currently available for use.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Sort order within category for UI display.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Record creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// Collection of athlete benchmarks for this definition.
    /// </summary>
    public ICollection<AthleteBenchmark> AthleteBenchmarks { get; set; } = new List<AthleteBenchmark>();

    /// <summary>
    /// Collection of movement mappings for this benchmark.
    /// </summary>
    public ICollection<BenchmarkMovementMapping> MovementMappings { get; set; } = new List<BenchmarkMovementMapping>();

    /// <summary>
    /// Collection of population percentiles for this benchmark.
    /// </summary>
    public ICollection<PopulationBenchmarkPercentile> PopulationPercentiles { get; set; } = new List<PopulationBenchmarkPercentile>();
}
