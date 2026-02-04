namespace WodStrat.Api.ViewModels.TimeEstimate;

/// <summary>
/// Response model for EMOM workout feasibility analysis.
/// </summary>
public class EmomFeasibilityResponse
{
    /// <summary>
    /// The workout's unique identifier.
    /// </summary>
    /// <example>5</example>
    public int WorkoutId { get; set; }

    /// <summary>
    /// Workout name.
    /// </summary>
    /// <example>EMOM 12</example>
    public string? WorkoutName { get; set; }

    /// <summary>
    /// Total duration of the EMOM in minutes.
    /// </summary>
    /// <example>12</example>
    public int TotalMinutes { get; set; }

    /// <summary>
    /// Whether the entire EMOM is feasible for this athlete.
    /// </summary>
    /// <example>true</example>
    public bool OverallFeasible { get; set; }

    /// <summary>
    /// Summary assessment of the EMOM feasibility.
    /// </summary>
    /// <example>This EMOM is feasible with consistent 15-20 second buffers. Focus on steady pacing for the power cleans.</example>
    public string OverallAssessment { get; set; } = string.Empty;

    /// <summary>
    /// Per-minute feasibility breakdown.
    /// </summary>
    public List<EmomMinuteResponse> MinuteBreakdown { get; set; } = new();

    /// <summary>
    /// Timestamp when the feasibility was calculated.
    /// </summary>
    /// <example>2024-01-15T10:30:00Z</example>
    public DateTime CalculatedAt { get; set; }
}
