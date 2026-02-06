namespace WodStrat.Services.Dtos;

/// <summary>
/// Unified movement context containing all shared movement data.
/// This eliminates duplication across pacing, volume, and time estimate responses.
/// </summary>
public class MovementContextDto
{
    /// <summary>
    /// Movement definition ID (primary key for lookups).
    /// </summary>
    public int MovementDefinitionId { get; set; }

    /// <summary>
    /// Display name of the movement.
    /// </summary>
    public string MovementName { get; set; } = string.Empty;

    /// <summary>
    /// Canonical name for system references.
    /// </summary>
    public string CanonicalName { get; set; } = string.Empty;

    /// <summary>
    /// Movement category (Gymnastics, Weightlifting, Cardio, etc.).
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a bodyweight movement.
    /// </summary>
    public bool IsBodyweight { get; set; }

    /// <summary>
    /// Name of the benchmark used for athlete comparison.
    /// </summary>
    public string BenchmarkUsed { get; set; } = "None";

    /// <summary>
    /// Athlete's percentile ranking for this movement (0-100).
    /// Null if no benchmark data available.
    /// </summary>
    public decimal? AthletePercentile { get; set; }

    /// <summary>
    /// Whether population data was available for calculation.
    /// </summary>
    public bool HasPopulationData { get; set; }

    /// <summary>
    /// Whether the athlete has recorded the relevant benchmark.
    /// </summary>
    public bool HasAthleteBenchmark { get; set; }
}
