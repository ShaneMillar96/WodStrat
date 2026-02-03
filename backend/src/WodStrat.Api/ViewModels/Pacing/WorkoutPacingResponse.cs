namespace WodStrat.Api.ViewModels.Pacing;

/// <summary>
/// Response model for workout pacing recommendations.
/// </summary>
public class WorkoutPacingResponse
{
    /// <summary>
    /// The workout's unique identifier.
    /// </summary>
    /// <example>5</example>
    public int WorkoutId { get; set; }

    /// <summary>
    /// Optional workout name.
    /// </summary>
    /// <example>Cindy</example>
    public string? WorkoutName { get; set; }

    /// <summary>
    /// Pacing recommendations for each movement in the workout.
    /// </summary>
    public List<MovementPacingResponse> MovementPacing { get; set; } = new();

    /// <summary>
    /// General workout strategy and approach recommendations.
    /// </summary>
    /// <example>Focus on maintaining consistent pacing through movements. Break pull-ups early to avoid fatigue.</example>
    public string OverallStrategyNotes { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the pacing was calculated.
    /// </summary>
    /// <example>2024-01-15T10:30:00Z</example>
    public DateTime CalculatedAt { get; set; }
}
