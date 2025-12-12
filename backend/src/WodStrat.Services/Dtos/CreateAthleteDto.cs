namespace WodStrat.Services.Dtos;

/// <summary>
/// Data transfer object for creating a new athlete profile.
/// </summary>
public class CreateAthleteDto
{
    /// <summary>
    /// Athlete's display name. Required, 1-100 characters.
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
    /// Experience level (Beginner/Intermediate/Advanced). Defaults to Intermediate.
    /// </summary>
    public string? ExperienceLevel { get; set; }

    /// <summary>
    /// Primary training goal. Defaults to ImprovePacing.
    /// </summary>
    public string? PrimaryGoal { get; set; }
}
