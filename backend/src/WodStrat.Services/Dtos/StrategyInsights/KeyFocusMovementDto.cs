namespace WodStrat.Services.Dtos;

/// <summary>
/// A movement requiring special strategic attention.
/// </summary>
public class KeyFocusMovementDto
{
    /// <summary>
    /// Movement definition ID for reference.
    /// </summary>
    public int MovementDefinitionId { get; set; }

    /// <summary>
    /// Display name of the movement.
    /// </summary>
    public string MovementName { get; set; } = string.Empty;

    /// <summary>
    /// Reason this movement needs focus.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Specific recommendation for this movement.
    /// </summary>
    public string Recommendation { get; set; } = string.Empty;

    /// <summary>
    /// Priority ranking (1 = highest priority).
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// The pacing level for this movement.
    /// </summary>
    public string PacingLevel { get; set; } = string.Empty;

    /// <summary>
    /// The volume load classification for this movement.
    /// </summary>
    public string LoadClassification { get; set; } = string.Empty;

    /// <summary>
    /// Whether scaling is recommended for this movement.
    /// </summary>
    public bool ScalingRecommended { get; set; }
}
