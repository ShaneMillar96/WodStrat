namespace WodStrat.Services.Dtos;

/// <summary>
/// Data transfer object for updating an existing athlete profile.
/// </summary>
public class UpdateAthleteDto
{
    /// <summary>
    /// Athlete's display name. 1-100 characters.
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
    /// Experience level (Beginner/Intermediate/Advanced).
    /// </summary>
    public string ExperienceLevel { get; set; } = string.Empty;

    /// <summary>
    /// Primary training goal.
    /// </summary>
    public string PrimaryGoal { get; set; } = string.Empty;
}
