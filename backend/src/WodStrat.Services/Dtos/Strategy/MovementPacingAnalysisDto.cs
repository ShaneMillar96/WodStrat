namespace WodStrat.Services.Dtos;

/// <summary>
/// Pacing analysis for a movement, referencing shared context by ID.
/// This is a slim version that avoids duplicating movement data.
/// </summary>
public class MovementPacingAnalysisDto
{
    /// <summary>
    /// Reference to movement in the shared context.
    /// </summary>
    public int MovementDefinitionId { get; set; }

    /// <summary>
    /// Calculated pacing level (Light/Moderate/Heavy).
    /// </summary>
    public string PacingLevel { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable pacing guidance text.
    /// </summary>
    public string GuidanceText { get; set; } = string.Empty;

    /// <summary>
    /// Recommended rep breakdown per set.
    /// </summary>
    public int[] RecommendedSets { get; set; } = Array.Empty<int>();
}
