namespace WodStrat.Services.Dtos;

/// <summary>
/// Pace target for a cardio movement.
/// </summary>
public class CardioPaceDto
{
    /// <summary>
    /// Primary pace display string (e.g., "4:35 /km" for running, "1:49 /500m" for rowing).
    /// </summary>
    public string DisplayPrimary { get; set; } = string.Empty;

    /// <summary>
    /// Secondary pace display string (e.g., "7:23 /mi" for running). Null for rowing.
    /// </summary>
    public string? DisplaySecondary { get; set; }

    /// <summary>
    /// Raw pace value in seconds per unit (for programmatic use).
    /// For running: seconds per km. For rowing: seconds per 500m.
    /// </summary>
    public decimal ValuePerUnit { get; set; }

    /// <summary>
    /// The pace unit descriptor (e.g., "km", "mi", "500m").
    /// </summary>
    public string PaceUnit { get; set; } = string.Empty;

    /// <summary>
    /// Whether this pace was derived from actual athlete benchmark data
    /// or is a generic recommendation.
    /// </summary>
    public bool IsDerivedFromBenchmark { get; set; }
}
