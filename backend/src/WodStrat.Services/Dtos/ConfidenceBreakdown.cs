namespace WodStrat.Services.Dtos;

/// <summary>
/// Detailed breakdown of confidence scoring factors.
/// </summary>
public class ConfidenceBreakdown
{
    /// <summary>
    /// Confidence in workout type detection (0-100).
    /// </summary>
    public int WorkoutTypeConfidence { get; set; }

    /// <summary>
    /// Confidence in time domain extraction (0-100).
    /// </summary>
    public int TimeDomainConfidence { get; set; }

    /// <summary>
    /// Average confidence in movement identification (0-100).
    /// </summary>
    public int MovementIdentificationConfidence { get; set; }

    /// <summary>
    /// Number of movements successfully identified.
    /// </summary>
    public int MovementsIdentified { get; set; }

    /// <summary>
    /// Total number of movement lines parsed.
    /// </summary>
    public int TotalMovementLines { get; set; }

    /// <summary>
    /// Number of movements with complete data (reps/distance/calories + load if applicable).
    /// </summary>
    public int MovementsWithCompleteData { get; set; }

    /// <summary>
    /// Percentage of movements identified.
    /// </summary>
    public decimal MovementIdentificationRate =>
        TotalMovementLines > 0 ? (decimal)MovementsIdentified / TotalMovementLines * 100 : 0;
}
