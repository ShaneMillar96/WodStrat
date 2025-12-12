using WodStrat.Dal.Enums;

namespace WodStrat.Dal.Models;

/// <summary>
/// Represents an athlete profile in the WodStrat system.
/// </summary>
public class Athlete : EntityBase
{
    /// <summary>
    /// Future foreign key to the users table (authentication Phase 2+).
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Athlete's display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Date of birth for age calculation.
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// Gender identity (Male/Female/Other/Prefer not to say).
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Height in centimeters.
    /// </summary>
    public decimal? HeightCm { get; set; }

    /// <summary>
    /// Weight in kilograms.
    /// </summary>
    public decimal? WeightKg { get; set; }

    /// <summary>
    /// Functional fitness experience level.
    /// </summary>
    public ExperienceLevel ExperienceLevel { get; set; } = ExperienceLevel.Intermediate;

    /// <summary>
    /// Primary training goal.
    /// </summary>
    public AthleteGoal PrimaryGoal { get; set; } = AthleteGoal.ImprovePacing;

    // Navigation properties
    /// <summary>
    /// Collection of benchmark results for this athlete.
    /// </summary>
    public ICollection<AthleteBenchmark> Benchmarks { get; set; } = new List<AthleteBenchmark>();
}
