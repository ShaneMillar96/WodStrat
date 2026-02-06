namespace WodStrat.Services.Dtos;

/// <summary>
/// Slim volume load analysis for unified strategy response.
/// </summary>
public class VolumeLoadAnalysisSummaryDto
{
    /// <summary>
    /// Per-movement volume analysis (references context by ID).
    /// </summary>
    public IReadOnlyList<MovementVolumeAnalysisDto> Movements { get; set; } = Array.Empty<MovementVolumeAnalysisDto>();

    /// <summary>
    /// Total volume load.
    /// </summary>
    public decimal TotalVolumeLoad { get; set; }

    /// <summary>
    /// Formatted total volume load.
    /// </summary>
    public string TotalVolumeLoadFormatted { get; set; } = string.Empty;

    /// <summary>
    /// Overall assessment.
    /// </summary>
    public string OverallAssessment { get; set; } = string.Empty;

    /// <summary>
    /// Volume distribution summary.
    /// </summary>
    public VolumeLoadDistributionDto Distribution { get; set; } = new();

    /// <summary>
    /// Whether all movements had sufficient data.
    /// </summary>
    public bool IsComplete { get; set; }
}
