namespace WodStrat.Services.Dtos;

/// <summary>
/// Feasibility analysis for a single EMOM minute.
/// </summary>
public class EmomFeasibilityDto
{
    /// <summary>
    /// The minute number (1-indexed).
    /// </summary>
    public int Minute { get; set; }

    /// <summary>
    /// Description of the prescribed work for this minute.
    /// </summary>
    public string PrescribedWork { get; set; } = string.Empty;

    /// <summary>
    /// Estimated time to complete the work in seconds.
    /// </summary>
    public int EstimatedCompletionSeconds { get; set; }

    /// <summary>
    /// Whether the athlete can feasibly complete the work with adequate rest.
    /// </summary>
    public bool IsFeasible { get; set; }

    /// <summary>
    /// Remaining seconds in the minute after completing work.
    /// </summary>
    public int BufferSeconds { get; set; }

    /// <summary>
    /// Recommendation for this minute (e.g., "On pace", "Consider scaling").
    /// </summary>
    public string Recommendation { get; set; } = string.Empty;

    /// <summary>
    /// Movement definitions involved in this minute.
    /// </summary>
    public IReadOnlyList<string> MovementNames { get; set; } = Array.Empty<string>();
}
