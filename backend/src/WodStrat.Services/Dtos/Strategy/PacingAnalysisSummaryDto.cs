namespace WodStrat.Services.Dtos;

/// <summary>
/// Slim pacing analysis for unified strategy response.
/// </summary>
public class PacingAnalysisSummaryDto
{
    /// <summary>
    /// Per-movement pacing analysis (references context by ID).
    /// </summary>
    public IReadOnlyList<MovementPacingAnalysisDto> Movements { get; set; } = Array.Empty<MovementPacingAnalysisDto>();

    /// <summary>
    /// Overall strategy notes.
    /// </summary>
    public string OverallStrategyNotes { get; set; } = string.Empty;

    /// <summary>
    /// Pacing distribution summary.
    /// </summary>
    public PacingDistributionDto Distribution { get; set; } = new();

    /// <summary>
    /// Whether all movements had sufficient data.
    /// </summary>
    public bool IsComplete { get; set; }
}
