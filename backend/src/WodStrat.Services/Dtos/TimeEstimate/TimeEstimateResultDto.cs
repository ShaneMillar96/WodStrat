namespace WodStrat.Services.Dtos;

/// <summary>
/// Complete time estimation result for a workout.
/// </summary>
public class TimeEstimateResultDto
{
    /// <summary>
    /// Reference to the workout.
    /// </summary>
    public int WorkoutId { get; set; }

    /// <summary>
    /// Display name of the workout.
    /// </summary>
    public string WorkoutName { get; set; } = string.Empty;

    /// <summary>
    /// Type of workout (AMRAP, ForTime, EMOM, Intervals).
    /// </summary>
    public string WorkoutType { get; set; } = string.Empty;

    /// <summary>
    /// Type of estimate provided: "Time" for ForTime, "RoundsReps" for AMRAP.
    /// </summary>
    public string EstimateType { get; set; } = string.Empty;

    /// <summary>
    /// Minimum estimate value (seconds for time, rounds for AMRAP).
    /// </summary>
    public int MinEstimate { get; set; }

    /// <summary>
    /// Maximum estimate value (seconds for time, rounds for AMRAP).
    /// </summary>
    public int MaxEstimate { get; set; }

    /// <summary>
    /// Additional reps beyond complete rounds (AMRAP only).
    /// </summary>
    public int? MinExtraReps { get; set; }

    /// <summary>
    /// Additional reps beyond complete rounds (AMRAP only).
    /// </summary>
    public int? MaxExtraReps { get; set; }

    /// <summary>
    /// Human-readable formatted range (e.g., "8:30 - 10:15" or "4+12 to 5+8 rounds").
    /// </summary>
    public string FormattedRange { get; set; } = string.Empty;

    /// <summary>
    /// Confidence level of the estimate: High, Medium, or Low.
    /// </summary>
    public string ConfidenceLevel { get; set; } = string.Empty;

    /// <summary>
    /// Summary of factors affecting the estimate.
    /// </summary>
    public string FactorsSummary { get; set; } = string.Empty;

    /// <summary>
    /// Per-movement rest recommendations based on pacing level.
    /// </summary>
    public IReadOnlyList<RestRecommendationDto> RestRecommendations { get; set; } = Array.Empty<RestRecommendationDto>();

    /// <summary>
    /// EMOM-specific feasibility analysis (null for non-EMOM workouts).
    /// </summary>
    public IReadOnlyList<EmomFeasibilityDto>? EmomFeasibility { get; set; }

    /// <summary>
    /// Timestamp when the estimate was calculated.
    /// </summary>
    public DateTime CalculatedAt { get; set; }

    /// <summary>
    /// Number of movements with benchmark coverage (used for confidence).
    /// </summary>
    public int BenchmarkCoverageCount { get; set; }

    /// <summary>
    /// Total movements in the workout.
    /// </summary>
    public int TotalMovementCount { get; set; }

    /// <summary>
    /// Average athlete percentile across covered movements.
    /// </summary>
    public decimal AveragePercentile { get; set; }
}
