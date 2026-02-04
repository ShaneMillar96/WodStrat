namespace WodStrat.Api.ViewModels.TimeEstimate;

/// <summary>
/// Response model for rest recommendation during a workout.
/// </summary>
public class RestRecommendationResponse
{
    /// <summary>
    /// The movement definition ID.
    /// </summary>
    /// <example>5</example>
    public int MovementDefinitionId { get; set; }

    /// <summary>
    /// The movement after which to take rest.
    /// </summary>
    /// <example>Pull-Up</example>
    public string AfterMovement { get; set; } = string.Empty;

    /// <summary>
    /// Suggested rest duration in seconds.
    /// </summary>
    /// <example>10</example>
    public int SuggestedRestSeconds { get; set; }

    /// <summary>
    /// Human-readable rest range.
    /// </summary>
    /// <example>8-12 seconds</example>
    public string RestRange { get; set; } = string.Empty;

    /// <summary>
    /// Explanation for why rest is recommended at this point.
    /// </summary>
    /// <example>Pull-ups are a relative weakness - quick rest will help maintain rep quality.</example>
    public string Reasoning { get; set; } = string.Empty;

    /// <summary>
    /// The pacing level for this movement (Light/Moderate/Heavy).
    /// </summary>
    /// <example>Moderate</example>
    public string PacingLevel { get; set; } = string.Empty;
}
