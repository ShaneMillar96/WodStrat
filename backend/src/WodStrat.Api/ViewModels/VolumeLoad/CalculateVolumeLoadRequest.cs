namespace WodStrat.Api.ViewModels.VolumeLoad;

/// <summary>
/// Request model for calculating workout volume load.
/// </summary>
public class CalculateVolumeLoadRequest
{
    /// <summary>
    /// The athlete's unique identifier.
    /// </summary>
    /// <example>1</example>
    public int AthleteId { get; set; }

    /// <summary>
    /// The workout's unique identifier.
    /// </summary>
    /// <example>5</example>
    public int WorkoutId { get; set; }
}
