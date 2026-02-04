namespace WodStrat.Services.Dtos;

/// <summary>
/// Breakdown of difficulty score contributing factors.
/// </summary>
public class DifficultyBreakdownDto
{
    /// <summary>
    /// Pacing factor contribution (0-10 scale, weight: 40%).
    /// Higher values indicate more movements are athlete weaknesses.
    /// </summary>
    public decimal PacingFactor { get; set; }

    /// <summary>
    /// Volume factor contribution (0-10 scale, weight: 30%).
    /// Higher values indicate heavier relative loads.
    /// </summary>
    public decimal VolumeFactor { get; set; }

    /// <summary>
    /// Time factor contribution (0-10 scale, weight: 30%).
    /// Higher values indicate longer/more demanding workouts.
    /// </summary>
    public decimal TimeFactor { get; set; }

    /// <summary>
    /// Experience level modifier applied to base score.
    /// Beginner = 1.2, Intermediate = 1.0, Advanced = 0.85.
    /// </summary>
    public decimal ExperienceModifier { get; set; }

    /// <summary>
    /// Raw base score before experience modifier.
    /// </summary>
    public decimal BaseScore { get; set; }

    /// <summary>
    /// Human-readable explanation of the calculation.
    /// </summary>
    public string Explanation { get; set; } = string.Empty;
}
