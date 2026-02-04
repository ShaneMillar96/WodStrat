namespace WodStrat.Services.Dtos;

/// <summary>
/// Request parameters for calculating strategy insights.
/// </summary>
public class StrategyInsightsRequestDto
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
