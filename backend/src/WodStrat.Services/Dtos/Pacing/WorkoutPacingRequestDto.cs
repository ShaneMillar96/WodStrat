namespace WodStrat.Services.Dtos;

/// <summary>
/// Request parameters for calculating workout pacing.
/// </summary>
public class WorkoutPacingRequestDto
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
