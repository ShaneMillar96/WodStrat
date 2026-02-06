namespace WodStrat.Services.Dtos;

/// <summary>
/// Slim time estimate analysis for unified strategy response.
/// </summary>
public class TimeEstimateAnalysisSummaryDto
{
    /// <summary>
    /// Type of estimate (Time or RoundsReps).
    /// </summary>
    public string EstimateType { get; set; } = string.Empty;

    /// <summary>
    /// Minimum estimate.
    /// </summary>
    public int MinEstimate { get; set; }

    /// <summary>
    /// Maximum estimate.
    /// </summary>
    public int MaxEstimate { get; set; }

    /// <summary>
    /// Extra reps for AMRAP min.
    /// </summary>
    public int? MinExtraReps { get; set; }

    /// <summary>
    /// Extra reps for AMRAP max.
    /// </summary>
    public int? MaxExtraReps { get; set; }

    /// <summary>
    /// Formatted range string.
    /// </summary>
    public string FormattedRange { get; set; } = string.Empty;

    /// <summary>
    /// Confidence level.
    /// </summary>
    public string ConfidenceLevel { get; set; } = string.Empty;

    /// <summary>
    /// Factors summary.
    /// </summary>
    public string FactorsSummary { get; set; } = string.Empty;

    /// <summary>
    /// Rest recommendations (references movement by ID).
    /// </summary>
    public IReadOnlyList<RestRecommendationSummaryDto> RestRecommendations { get; set; } = Array.Empty<RestRecommendationSummaryDto>();

    /// <summary>
    /// EMOM feasibility (if applicable).
    /// </summary>
    public IReadOnlyList<EmomFeasibilityDto>? EmomFeasibility { get; set; }

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
