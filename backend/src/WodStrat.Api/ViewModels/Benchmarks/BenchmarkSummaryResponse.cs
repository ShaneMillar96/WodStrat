namespace WodStrat.Api.ViewModels.Benchmarks;

/// <summary>
/// Response model for athlete benchmark summary with completeness statistics.
/// </summary>
public class BenchmarkSummaryResponse
{
    /// <summary>
    /// Reference to the athlete.
    /// </summary>
    /// <example>7c9e6679-7425-40de-944b-e07fc1f90ae7</example>
    public Guid AthleteId { get; set; }

    /// <summary>
    /// Total number of recorded benchmarks.
    /// </summary>
    /// <example>8</example>
    public int TotalBenchmarks { get; set; }

    /// <summary>
    /// True if athlete has met the minimum benchmark requirement.
    /// </summary>
    /// <example>true</example>
    public bool MeetsMinimumRequirement { get; set; }

    /// <summary>
    /// The minimum number of benchmarks required.
    /// </summary>
    /// <example>3</example>
    public int MinimumRequired { get; set; }

    /// <summary>
    /// Breakdown of benchmarks by category.
    /// </summary>
    public Dictionary<string, int> BenchmarksByCategory { get; set; } = new();

    /// <summary>
    /// All athlete benchmarks with full details.
    /// </summary>
    public List<AthleteBenchmarkResponse> Benchmarks { get; set; } = new();
}
