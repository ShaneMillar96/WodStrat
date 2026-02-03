namespace WodStrat.Services.Dtos;

/// <summary>
/// Distribution summary of load classifications within a workout.
/// </summary>
public class VolumeLoadDistributionDto
{
    /// <summary>
    /// Count of movements with High load classification.
    /// </summary>
    public int HighCount { get; set; }

    /// <summary>
    /// Count of movements with Moderate load classification.
    /// </summary>
    public int ModerateCount { get; set; }

    /// <summary>
    /// Count of movements with Low load classification.
    /// </summary>
    public int LowCount { get; set; }

    /// <summary>
    /// Count of movements without external load (bodyweight).
    /// </summary>
    public int BodyweightCount { get; set; }

    /// <summary>
    /// Total movements analyzed.
    /// </summary>
    public int TotalMovements { get; set; }

    /// <summary>
    /// Count of movements with insufficient benchmark data.
    /// </summary>
    public int InsufficientDataCount { get; set; }
}
