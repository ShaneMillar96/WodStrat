namespace WodStrat.Api.ViewModels.Athletes;

/// <summary>
/// Response model for athlete profile data.
/// </summary>
public class AthleteResponse
{
    /// <summary>
    /// Unique identifier for the athlete.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Athlete's display name.
    /// </summary>
    /// <example>John Doe</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Calculated age from date of birth. Null if DOB not provided.
    /// </summary>
    /// <example>34</example>
    public int? Age { get; set; }

    /// <summary>
    /// Gender identity.
    /// </summary>
    /// <example>Male</example>
    public string? Gender { get; set; }

    /// <summary>
    /// Height in centimeters.
    /// </summary>
    /// <example>180.5</example>
    public decimal? HeightCm { get; set; }

    /// <summary>
    /// Weight in kilograms.
    /// </summary>
    /// <example>82.3</example>
    public decimal? WeightKg { get; set; }

    /// <summary>
    /// Functional fitness experience level.
    /// </summary>
    /// <example>Intermediate</example>
    public string ExperienceLevel { get; set; } = string.Empty;

    /// <summary>
    /// Primary training goal.
    /// </summary>
    /// <example>ImprovePacing</example>
    public string PrimaryGoal { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the profile was created.
    /// </summary>
    /// <example>2024-01-15T10:30:00Z</example>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the profile was last updated.
    /// </summary>
    /// <example>2024-01-20T14:45:00Z</example>
    public DateTime UpdatedAt { get; set; }
}
