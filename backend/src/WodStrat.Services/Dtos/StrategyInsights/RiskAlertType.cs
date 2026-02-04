namespace WodStrat.Services.Dtos;

/// <summary>
/// Risk alert type identifiers.
/// </summary>
public static class RiskAlertType
{
    /// <summary>
    /// Multiple movements with high volume and light pacing - consider scaling.
    /// </summary>
    public const string ScalingRecommended = "ScalingRecommended";

    /// <summary>
    /// Estimated completion time approaches or exceeds time cap.
    /// </summary>
    public const string TimeCapRisk = "TimeCapRisk";

    /// <summary>
    /// High difficulty combined with high volume - recovery impact concern.
    /// </summary>
    public const string RecoveryImpact = "RecoveryImpact";

    /// <summary>
    /// Mixed pacing across movements may require strategy adjustments.
    /// </summary>
    public const string PacingMismatch = "PacingMismatch";

    /// <summary>
    /// Key movement lacks benchmark data for accurate recommendations.
    /// </summary>
    public const string BenchmarkGap = "BenchmarkGap";
}
