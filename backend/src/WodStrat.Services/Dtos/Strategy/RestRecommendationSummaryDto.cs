namespace WodStrat.Services.Dtos;

/// <summary>
/// Slim rest recommendation referencing movement by ID.
/// </summary>
public class RestRecommendationSummaryDto
{
    /// <summary>
    /// Reference to movement in context.
    /// </summary>
    public int MovementDefinitionId { get; set; }

    /// <summary>
    /// Suggested rest in seconds.
    /// </summary>
    public int SuggestedRestSeconds { get; set; }

    /// <summary>
    /// Rest range string.
    /// </summary>
    public string RestRange { get; set; } = string.Empty;

    /// <summary>
    /// Reasoning.
    /// </summary>
    public string Reasoning { get; set; } = string.Empty;

    /// <summary>
    /// Pacing level.
    /// </summary>
    public string PacingLevel { get; set; } = string.Empty;
}
