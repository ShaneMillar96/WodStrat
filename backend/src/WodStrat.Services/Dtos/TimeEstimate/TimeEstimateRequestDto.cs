namespace WodStrat.Services.Dtos;

/// <summary>
/// Request parameters for workout time estimation.
/// </summary>
public class TimeEstimateRequestDto
{
    /// <summary>
    /// The athlete's unique identifier.
    /// </summary>
    public int AthleteId { get; set; }

    /// <summary>
    /// The workout's unique identifier.
    /// </summary>
    public int WorkoutId { get; set; }
}
