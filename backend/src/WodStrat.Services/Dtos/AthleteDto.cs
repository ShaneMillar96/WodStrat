namespace WodStrat.Services.Dtos;

/// <summary>
/// Data transfer object for athlete profile responses.
/// </summary>
public class AthleteDto
{
    /// <summary>
    /// Unique identifier for the athlete.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Athlete's display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Calculated age from date of birth. Null if date of birth not provided.
    /// </summary>
    public int? Age { get; set; }

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
    /// Experience level (Beginner/Intermediate/Advanced).
    /// </summary>
    public string ExperienceLevel { get; set; } = string.Empty;

    /// <summary>
    /// Primary training goal.
    /// </summary>
    public string PrimaryGoal { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the profile was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the profile was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
