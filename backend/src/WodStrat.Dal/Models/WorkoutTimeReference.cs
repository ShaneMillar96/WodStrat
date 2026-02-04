using WodStrat.Dal.Enums;

namespace WodStrat.Dal.Models;

/// <summary>
/// Stores reference completion times for known workouts (Hero/Girl WODs)
/// at various population percentile levels for time estimation purposes.
/// </summary>
public class WorkoutTimeReference
{
    /// <summary>
    /// Unique auto-incrementing identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the workout (e.g., "Fran", "Grace", "Helen").
    /// </summary>
    public string WorkoutName { get; set; } = string.Empty;

    /// <summary>
    /// Completion time at 20th percentile (slower end) in seconds.
    /// </summary>
    public int Percentile20Seconds { get; set; }

    /// <summary>
    /// Completion time at 40th percentile in seconds.
    /// </summary>
    public int Percentile40Seconds { get; set; }

    /// <summary>
    /// Completion time at 60th percentile in seconds.
    /// </summary>
    public int Percentile60Seconds { get; set; }

    /// <summary>
    /// Completion time at 80th percentile in seconds.
    /// </summary>
    public int Percentile80Seconds { get; set; }

    /// <summary>
    /// Completion time at 95th percentile (elite) in seconds.
    /// </summary>
    public int Percentile95Seconds { get; set; }

    /// <summary>
    /// Gender filter for segmented percentiles (null = all genders).
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Experience level filter for segmented percentiles (null = all levels).
    /// </summary>
    public ExperienceLevel? ExperienceLevel { get; set; }

    /// <summary>
    /// Record creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
