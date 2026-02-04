namespace WodStrat.Services.Dtos;

/// <summary>
/// Rest recommendation after completing a movement.
/// </summary>
public class RestRecommendationDto
{
    /// <summary>
    /// Movement name to rest after.
    /// </summary>
    public string AfterMovement { get; set; } = string.Empty;

    /// <summary>
    /// Movement definition ID for reference.
    /// </summary>
    public int MovementDefinitionId { get; set; }

    /// <summary>
    /// Suggested rest duration in seconds.
    /// </summary>
    public int SuggestedRestSeconds { get; set; }

    /// <summary>
    /// Formatted rest range (e.g., "8-12 seconds").
    /// </summary>
    public string RestRange { get; set; } = string.Empty;

    /// <summary>
    /// Explanation for the rest recommendation.
    /// </summary>
    public string Reasoning { get; set; } = string.Empty;

    /// <summary>
    /// The pacing level for this movement (Light/Moderate/Heavy).
    /// </summary>
    public string PacingLevel { get; set; } = string.Empty;
}
