namespace WodStrat.Services.Dtos;

/// <summary>
/// Unified workout context containing shared workout metadata.
/// </summary>
public class WorkoutContextDto
{
    /// <summary>
    /// Workout unique identifier.
    /// </summary>
    public int WorkoutId { get; set; }

    /// <summary>
    /// Display name of the workout.
    /// </summary>
    public string WorkoutName { get; set; } = string.Empty;

    /// <summary>
    /// Type of workout (AMRAP, ForTime, EMOM, etc.).
    /// </summary>
    public string WorkoutType { get; set; } = string.Empty;

    /// <summary>
    /// Time cap in seconds (if applicable).
    /// </summary>
    public int? TimeCapSeconds { get; set; }

    /// <summary>
    /// Number of rounds (if applicable).
    /// </summary>
    public int? RoundCount { get; set; }

    /// <summary>
    /// Movement contexts for all movements in this workout.
    /// </summary>
    public IReadOnlyList<MovementContextDto> Movements { get; set; } = Array.Empty<MovementContextDto>();

    /// <summary>
    /// Timestamp when the analysis was calculated.
    /// </summary>
    public DateTime CalculatedAt { get; set; }
}
