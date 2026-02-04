namespace WodStrat.Api.ViewModels.TimeEstimate;

/// <summary>
/// Response model for workout time estimate.
/// </summary>
public class TimeEstimateResponse
{
    /// <summary>
    /// The workout's unique identifier.
    /// </summary>
    /// <example>5</example>
    public int WorkoutId { get; set; }

    /// <summary>
    /// Workout name.
    /// </summary>
    /// <example>Fran</example>
    public string WorkoutName { get; set; } = string.Empty;

    /// <summary>
    /// Type of workout (ForTime, AMRAP, EMOM, etc.).
    /// </summary>
    /// <example>ForTime</example>
    public string WorkoutType { get; set; } = string.Empty;

    /// <summary>
    /// Type of estimate provided. "Time" for ForTime/EMOM workouts (in seconds),
    /// "RoundsReps" for AMRAP workouts.
    /// </summary>
    /// <example>Time</example>
    public string EstimateType { get; set; } = string.Empty;

    /// <summary>
    /// Minimum estimated value. For Time estimates, this is seconds.
    /// For RoundsReps, this is the minimum expected rounds.
    /// </summary>
    /// <example>180</example>
    public int MinEstimate { get; set; }

    /// <summary>
    /// Maximum estimated value. For Time estimates, this is seconds.
    /// For RoundsReps, this is the maximum expected rounds.
    /// </summary>
    /// <example>240</example>
    public int MaxEstimate { get; set; }

    /// <summary>
    /// Additional reps beyond complete rounds (AMRAP only).
    /// </summary>
    /// <example>12</example>
    public int? MinExtraReps { get; set; }

    /// <summary>
    /// Additional reps beyond complete rounds (AMRAP only).
    /// </summary>
    /// <example>8</example>
    public int? MaxExtraReps { get; set; }

    /// <summary>
    /// Human-readable formatted estimate range.
    /// </summary>
    /// <example>3:00 - 4:00</example>
    public string FormattedRange { get; set; } = string.Empty;

    /// <summary>
    /// Confidence level in the estimate based on available benchmark data.
    /// </summary>
    /// <example>High</example>
    public string ConfidenceLevel { get; set; } = string.Empty;

    /// <summary>
    /// Summary of factors that influenced the estimate.
    /// </summary>
    /// <example>Strong pull-up performance (+15%), moderate thruster capacity, high aerobic capacity from rowing benchmarks.</example>
    public string FactorsSummary { get; set; } = string.Empty;

    /// <summary>
    /// Recommended rest periods during the workout.
    /// </summary>
    public List<RestRecommendationResponse> RestRecommendations { get; set; } = new();

    /// <summary>
    /// EMOM-specific feasibility analysis (null for non-EMOM workouts).
    /// </summary>
    public List<EmomMinuteResponse>? EmomFeasibility { get; set; }

    /// <summary>
    /// Timestamp when the time estimate was calculated.
    /// </summary>
    /// <example>2024-01-15T10:30:00Z</example>
    public DateTime CalculatedAt { get; set; }

    /// <summary>
    /// Number of movements with benchmark coverage.
    /// </summary>
    /// <example>4</example>
    public int BenchmarkCoverageCount { get; set; }

    /// <summary>
    /// Total movements in the workout.
    /// </summary>
    /// <example>5</example>
    public int TotalMovementCount { get; set; }

    /// <summary>
    /// Average athlete percentile across covered movements.
    /// </summary>
    /// <example>72.5</example>
    public decimal AveragePercentile { get; set; }
}
