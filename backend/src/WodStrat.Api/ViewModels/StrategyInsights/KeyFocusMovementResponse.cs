namespace WodStrat.Api.ViewModels.StrategyInsights;

/// <summary>
/// Response model for a key focus movement recommendation.
/// </summary>
public class KeyFocusMovementResponse
{
    /// <summary>
    /// Name of the movement.
    /// </summary>
    /// <example>Thrusters</example>
    public string MovementName { get; set; } = string.Empty;

    /// <summary>
    /// Reason why this movement requires special focus.
    /// </summary>
    /// <example>Weakness with high volume - this will be your limiter</example>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Strategic recommendation for approaching this movement.
    /// </summary>
    /// <example>Break into sets of 7-7-7 from the start, don't go out too fast</example>
    public string Recommendation { get; set; } = string.Empty;

    /// <summary>
    /// Priority ranking (1 = highest priority).
    /// </summary>
    /// <example>1</example>
    public int Priority { get; set; }
}
