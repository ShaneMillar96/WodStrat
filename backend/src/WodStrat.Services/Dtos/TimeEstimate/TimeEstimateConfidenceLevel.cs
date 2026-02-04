namespace WodStrat.Services.Dtos;

/// <summary>
/// Confidence level for time estimates based on benchmark data coverage.
/// </summary>
public static class TimeEstimateConfidenceLevel
{
    public const string High = "High";
    public const string Medium = "Medium";
    public const string Low = "Low";

    /// <summary>
    /// Minimum benchmark coverage percentage for High confidence.
    /// </summary>
    public const int HighThreshold = 80;

    /// <summary>
    /// Minimum benchmark coverage percentage for Medium confidence.
    /// </summary>
    public const int MediumThreshold = 50;

    /// <summary>
    /// Determines confidence level from benchmark coverage percentage.
    /// </summary>
    public static string FromCoverage(int coveragePercent) => coveragePercent switch
    {
        >= HighThreshold => High,
        >= MediumThreshold => Medium,
        _ => Low
    };
}
