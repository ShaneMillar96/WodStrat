namespace WodStrat.Api.ViewModels.Pacing;

/// <summary>
/// Request model for calculating workout pacing.
/// </summary>
public class CalculatePacingRequest
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
