using WodStrat.Dal.Enums;

namespace WodStrat.Services.Dtos;

/// <summary>
/// Data transfer object for a parsed movement within a workout.
/// </summary>
public class ParsedMovementDto
{
    /// <summary>
    /// Order of the movement within the workout (1-indexed).
    /// </summary>
    public int SequenceOrder { get; set; }

    /// <summary>
    /// Original text that was parsed for this movement.
    /// </summary>
    public string OriginalText { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the matched movement definition ID.
    /// Null if movement could not be identified.
    /// </summary>
    public int? MovementDefinitionId { get; set; }

    /// <summary>
    /// Display name of the matched movement.
    /// </summary>
    public string? MovementName { get; set; }

    /// <summary>
    /// Canonical name of the matched movement.
    /// </summary>
    public string? MovementCanonicalName { get; set; }

    /// <summary>
    /// Category of the matched movement.
    /// </summary>
    public string? MovementCategory { get; set; }

    /// <summary>
    /// Number of repetitions.
    /// </summary>
    public int? RepCount { get; set; }

    /// <summary>
    /// Weight/load amount (male standard if gender-differentiated).
    /// </summary>
    public decimal? LoadValue { get; set; }

    /// <summary>
    /// Weight/load amount for female athletes (if gender-differentiated).
    /// </summary>
    public decimal? LoadValueFemale { get; set; }

    /// <summary>
    /// Weight unit (kg/lb/pood).
    /// </summary>
    public LoadUnit? LoadUnit { get; set; }

    /// <summary>
    /// Distance amount.
    /// </summary>
    public decimal? DistanceValue { get; set; }

    /// <summary>
    /// Distance unit (m/km/ft/mi/cal).
    /// </summary>
    public DistanceUnit? DistanceUnit { get; set; }

    /// <summary>
    /// Calorie target (male standard if gender-differentiated).
    /// </summary>
    public int? Calories { get; set; }

    /// <summary>
    /// Calorie target for female athletes (if gender-differentiated).
    /// </summary>
    public int? CaloriesFemale { get; set; }

    /// <summary>
    /// Time duration in seconds (for holds, etc.).
    /// </summary>
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// Additional notes or specifications.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Rep scheme reps for this movement (e.g., [21, 15, 9] for "21-15-9").
    /// Applied when movement has no explicit rep count but workout has a rep scheme.
    /// </summary>
    public int[]? RepSchemeReps { get; set; }

    /// <summary>
    /// Type of rep scheme (Descending, Ascending, Fixed, Custom).
    /// </summary>
    public string? RepSchemeType { get; set; }
}
