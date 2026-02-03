namespace WodStrat.Services.Dtos;

/// <summary>
/// Distribution summary of pacing levels within a workout.
/// </summary>
public class PacingDistributionDto
{
    /// <summary>
    /// Count of movements with Heavy pacing.
    /// </summary>
    public int HeavyCount { get; set; }

    /// <summary>
    /// Count of movements with Moderate pacing.
    /// </summary>
    public int ModerateCount { get; set; }

    /// <summary>
    /// Count of movements with Light pacing.
    /// </summary>
    public int LightCount { get; set; }

    /// <summary>
    /// Total movements analyzed.
    /// </summary>
    public int TotalMovements { get; set; }

    /// <summary>
    /// Count of movements with incomplete data.
    /// </summary>
    public int IncompleteDataCount { get; set; }
}
