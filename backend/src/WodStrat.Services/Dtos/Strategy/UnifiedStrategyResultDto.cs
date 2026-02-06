namespace WodStrat.Services.Dtos;

/// <summary>
/// Unified strategy result that combines all analysis types with shared context.
/// This is the recommended response format for Strategy Page consumption.
/// </summary>
public class UnifiedStrategyResultDto
{
    /// <summary>
    /// Shared workout context with all movement data.
    /// </summary>
    public WorkoutContextDto Context { get; set; } = new();

    /// <summary>
    /// Pacing analysis (slim format referencing context).
    /// </summary>
    public PacingAnalysisSummaryDto Pacing { get; set; } = new();

    /// <summary>
    /// Volume load analysis (slim format referencing context).
    /// </summary>
    public VolumeLoadAnalysisSummaryDto VolumeLoad { get; set; } = new();

    /// <summary>
    /// Time estimate analysis.
    /// </summary>
    public TimeEstimateAnalysisSummaryDto TimeEstimate { get; set; } = new();

    /// <summary>
    /// Strategy insights (aggregated analysis).
    /// </summary>
    public StrategyInsightsSummaryDto Insights { get; set; } = new();
}
