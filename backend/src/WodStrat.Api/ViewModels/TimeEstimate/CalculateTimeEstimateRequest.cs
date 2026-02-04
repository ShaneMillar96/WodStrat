namespace WodStrat.Api.ViewModels.TimeEstimate;

/// <summary>
/// Request model for calculating workout time estimate.
/// </summary>
public class CalculateTimeEstimateRequest
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
