using WodStrat.Dal.Enums;

namespace WodStrat.Dal.Models;

/// <summary>
/// Stores population reference data for benchmark percentile calculations.
/// Supports gender and experience-level segmentation.
/// </summary>
public class PopulationBenchmarkPercentile
{
    /// <summary>
    /// Unique auto-incrementing identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Reference to the benchmark definition.
    /// </summary>
    public int BenchmarkDefinitionId { get; set; }

    /// <summary>
    /// Value at the 20th percentile.
    /// </summary>
    public decimal Percentile20 { get; set; }

    /// <summary>
    /// Value at the 40th percentile.
    /// </summary>
    public decimal Percentile40 { get; set; }

    /// <summary>
    /// Value at the 60th percentile.
    /// </summary>
    public decimal Percentile60 { get; set; }

    /// <summary>
    /// Value at the 80th percentile.
    /// </summary>
    public decimal Percentile80 { get; set; }

    /// <summary>
    /// Value at the 95th percentile.
    /// </summary>
    public decimal Percentile95 { get; set; }

    /// <summary>
    /// Gender filter for segmented percentiles (null = all genders).
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Experience level filter for segmented percentiles (null = all levels).
    /// </summary>
    public ExperienceLevel? ExperienceLevel { get; set; }

    /// <summary>
    /// Record creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// The benchmark definition these percentiles apply to.
    /// </summary>
    public BenchmarkDefinition BenchmarkDefinition { get; set; } = null!;
}
