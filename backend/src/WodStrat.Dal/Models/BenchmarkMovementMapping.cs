namespace WodStrat.Dal.Models;

/// <summary>
/// Maps benchmark definitions to movement definitions for pacing calculations.
/// Indicates which benchmarks predict performance in which movements.
/// </summary>
public class BenchmarkMovementMapping
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
    /// Reference to the movement definition.
    /// </summary>
    public int MovementDefinitionId { get; set; }

    /// <summary>
    /// How strongly the benchmark predicts movement performance (0.0-1.0).
    /// 1.0 = strong correlation, 0.5 = moderate, lower = weak.
    /// </summary>
    public decimal RelevanceFactor { get; set; } = 1.0m;

    /// <summary>
    /// Record creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// The benchmark definition that predicts performance.
    /// </summary>
    public BenchmarkDefinition BenchmarkDefinition { get; set; } = null!;

    /// <summary>
    /// The movement definition whose performance is predicted.
    /// </summary>
    public MovementDefinition MovementDefinition { get; set; } = null!;
}
