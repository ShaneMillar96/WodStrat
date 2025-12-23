namespace WodStrat.Api.ViewModels.Workouts;

/// <summary>
/// Detailed breakdown of parsing confidence factors.
/// </summary>
public class ConfidenceBreakdownResponse
{
    /// <summary>
    /// Confidence in workout type detection (0.0 to 1.0).
    /// </summary>
    /// <example>0.95</example>
    public decimal WorkoutTypeConfidence { get; set; }

    /// <summary>
    /// Confidence in time domain extraction (0.0 to 1.0).
    /// </summary>
    /// <example>1.0</example>
    public decimal TimeDomainConfidence { get; set; }

    /// <summary>
    /// Average confidence in movement identification (0.0 to 1.0).
    /// </summary>
    /// <example>0.85</example>
    public decimal MovementIdentificationConfidence { get; set; }

    /// <summary>
    /// Number of movements successfully identified.
    /// </summary>
    /// <example>4</example>
    public int MovementsIdentified { get; set; }

    /// <summary>
    /// Total number of movement lines parsed.
    /// </summary>
    /// <example>5</example>
    public int TotalMovementLines { get; set; }

    /// <summary>
    /// Number of movements with complete data.
    /// </summary>
    /// <example>3</example>
    public int MovementsWithCompleteData { get; set; }

    /// <summary>
    /// Percentage of movements successfully identified.
    /// </summary>
    /// <example>80.0</example>
    public decimal MovementIdentificationRate { get; set; }
}
