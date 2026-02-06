namespace WodStrat.Services.Dtos;

/// <summary>
/// Pacing guidance for a single movement based on athlete capabilities.
/// </summary>
public class MovementPacingDto
{
    /// <summary>
    /// Reference to the movement definition.
    /// </summary>
    public int MovementDefinitionId { get; set; }

    /// <summary>
    /// Display name of the movement.
    /// </summary>
    public string MovementName { get; set; } = string.Empty;

    /// <summary>
    /// Calculated pacing level (Light/Moderate/Heavy).
    /// </summary>
    public string PacingLevel { get; set; } = string.Empty;

    /// <summary>
    /// Athlete's percentile ranking for this movement (0-100).
    /// </summary>
    public decimal AthletePercentile { get; set; }

    /// <summary>
    /// Human-readable pacing guidance text.
    /// </summary>
    public string GuidanceText { get; set; } = string.Empty;

    /// <summary>
    /// Recommended rep breakdown per set.
    /// </summary>
    public int[] RecommendedSets { get; set; } = Array.Empty<int>();

    /// <summary>
    /// Name of the benchmark used to determine pacing.
    /// </summary>
    public string BenchmarkUsed { get; set; } = string.Empty;

    /// <summary>
    /// Whether population data was available for calculation.
    /// False indicates default pacing was used.
    /// </summary>
    public bool HasPopulationData { get; set; }

    /// <summary>
    /// Whether the athlete has recorded this benchmark.
    /// False indicates default pacing was used.
    /// </summary>
    public bool HasAthleteBenchmark { get; set; }

    /// <summary>
    /// Whether this is a cardio/monostructural movement with pace-based guidance.
    /// When true, RecommendedSets will be empty and TargetPace may be populated.
    /// </summary>
    public bool IsCardio { get; set; }

    /// <summary>
    /// Target pace for cardio movements. Null for non-cardio movements.
    /// </summary>
    public CardioPaceDto? TargetPace { get; set; }
}
