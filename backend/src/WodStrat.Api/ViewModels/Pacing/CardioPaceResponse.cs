namespace WodStrat.Api.ViewModels.Pacing;

/// <summary>
/// Cardio pace information for pace-based movements (running, rowing, etc.).
/// </summary>
public class CardioPaceResponse
{
    /// <summary>
    /// Primary pace display string in the movement's natural unit.
    /// </summary>
    /// <example>4:35 /km</example>
    public string DisplayPrimary { get; set; } = string.Empty;

    /// <summary>
    /// Secondary pace display string in an alternate unit.
    /// Only populated for running (shows miles when primary is km, and vice versa).
    /// Null for rowing, skiing, biking, etc.
    /// </summary>
    /// <example>7:23 /mi</example>
    public string? DisplaySecondary { get; set; }

    /// <summary>
    /// Raw pace value in seconds per unit (e.g., 275.0 for 4:35/km).
    /// Useful for programmatic comparisons or custom formatting.
    /// </summary>
    /// <example>275.0</example>
    public decimal ValuePerUnit { get; set; }

    /// <summary>
    /// The pace unit descriptor (e.g., "/km", "/500m", "/mi").
    /// </summary>
    /// <example>/km</example>
    public string PaceUnit { get; set; } = string.Empty;
}
