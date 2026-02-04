namespace WodStrat.Api.ViewModels.TimeEstimate;

/// <summary>
/// Response model for individual EMOM minute feasibility.
/// </summary>
public class EmomMinuteResponse
{
    /// <summary>
    /// The minute number (1-based).
    /// </summary>
    /// <example>1</example>
    public int Minute { get; set; }

    /// <summary>
    /// Description of the prescribed work for this minute.
    /// </summary>
    /// <example>10 Power Cleans @ 60kg</example>
    public string PrescribedWork { get; set; } = string.Empty;

    /// <summary>
    /// Estimated time to complete the prescribed work in seconds.
    /// </summary>
    /// <example>42</example>
    public int EstimatedCompletionSeconds { get; set; }

    /// <summary>
    /// Whether the prescribed work can be completed within 60 seconds.
    /// </summary>
    /// <example>true</example>
    public bool IsFeasible { get; set; }

    /// <summary>
    /// Remaining seconds after completing the prescribed work (60 - completion time).
    /// Negative values indicate work exceeds the minute.
    /// </summary>
    /// <example>18</example>
    public int BufferSeconds { get; set; }

    /// <summary>
    /// Guidance for approaching this minute.
    /// </summary>
    /// <example>Maintain steady singles or quick doubles. You have adequate rest buffer.</example>
    public string Recommendation { get; set; } = string.Empty;

    /// <summary>
    /// Movement names involved in this minute.
    /// </summary>
    public IReadOnlyList<string> MovementNames { get; set; } = Array.Empty<string>();
}
