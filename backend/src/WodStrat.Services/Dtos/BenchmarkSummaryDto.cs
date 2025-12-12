namespace WodStrat.Services.Dtos;

/// <summary>
/// Data transfer object for benchmark summary with completeness statistics.
/// </summary>
public class BenchmarkSummaryDto
{
    /// <summary>
    /// Reference to the athlete.
    /// </summary>
    public Guid AthleteId { get; set; }

    /// <summary>
    /// Total number of recorded benchmarks.
    /// </summary>
    public int TotalBenchmarks { get; set; }

    /// <summary>
    /// True if athlete has met the minimum benchmark requirement (>= 3).
    /// </summary>
    public bool MeetsMinimumRequirement { get; set; }

    /// <summary>
    /// The minimum number of benchmarks required.
    /// </summary>
    public int MinimumRequired { get; set; } = 3;

    /// <summary>
    /// Count of benchmarks per category.
    /// </summary>
    public Dictionary<string, int> BenchmarksByCategory { get; set; } = new();

    /// <summary>
    /// All athlete benchmarks with full details.
    /// </summary>
    public IReadOnlyList<AthleteBenchmarkDto> Benchmarks { get; set; } = Array.Empty<AthleteBenchmarkDto>();
}
