namespace WodStrat.Api.ViewModels.Athletes;

/// <summary>
/// Request model for updating an existing athlete profile.
/// </summary>
public class UpdateAthleteRequest
{
    /// <summary>
    /// Athlete's display name.
    /// </summary>
    /// <example>John Doe</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Date of birth for age calculation (ISO 8601 format).
    /// </summary>
    /// <example>1990-05-15</example>
    public DateOnly? DateOfBirth { get; set; }

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
}
