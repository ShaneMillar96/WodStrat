namespace WodStrat.Services.Dtos;

/// <summary>
/// Key focus movement referencing context by ID.
/// </summary>
public class KeyFocusMovementSummaryDto
{
    /// <summary>
    /// Reference to movement in context.
    /// </summary>
    public int MovementDefinitionId { get; set; }

    /// <summary>
    /// Reason for focus.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Recommendation.
    /// </summary>
    public string Recommendation { get; set; } = string.Empty;

    /// <summary>
    /// Priority (1 = highest).
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Pacing level.
    /// </summary>
    public string PacingLevel { get; set; } = string.Empty;

    /// <summary>
    /// Load classification.
    /// </summary>
    public string LoadClassification { get; set; } = string.Empty;

    /// <summary>
    /// Whether scaling is recommended.
    /// </summary>
    public bool ScalingRecommended { get; set; }
}
