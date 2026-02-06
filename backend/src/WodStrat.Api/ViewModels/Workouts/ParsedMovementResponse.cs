namespace WodStrat.Api.ViewModels.Workouts;

/// <summary>
/// Response model for a parsed movement within a workout.
/// </summary>
public class ParsedMovementResponse
{
    /// <summary>
    /// Order of the movement within the workout (1-indexed).
    /// </summary>
    /// <example>1</example>
    public int SequenceOrder { get; set; }

    /// <summary>
    /// Original text that was parsed for this movement.
    /// </summary>
    /// <example>10 Pull-ups</example>
    public string OriginalText { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the matched movement definition ID.
    /// Null if movement could not be identified.
    /// </summary>
    /// <example>15</example>
    public int? MovementDefinitionId { get; set; }

    /// <summary>
    /// Display name of the matched movement.
    /// </summary>
    /// <example>Pull-Up</example>
    public string? MovementName { get; set; }

    /// <summary>
    /// Canonical name of the matched movement.
    /// </summary>
    /// <example>pull_up</example>
    public string? MovementCanonicalName { get; set; }

    /// <summary>
    /// Category of the matched movement.
    /// </summary>
    /// <example>Gymnastics</example>
    public string? MovementCategory { get; set; }

    /// <summary>
    /// Number of repetitions.
    /// </summary>
    /// <example>10</example>
    public int? RepCount { get; set; }

    /// <summary>
    /// Weight/load amount (male standard if gender-differentiated).
    /// </summary>
    /// <example>135</example>
    public decimal? LoadValue { get; set; }

    /// <summary>
    /// Weight/load amount for female athletes (if gender-differentiated).
    /// </summary>
    /// <example>95</example>
    public decimal? LoadValueFemale { get; set; }

    /// <summary>
    /// Weight unit.
    /// </summary>
    /// <example>Lb</example>
    public string? LoadUnit { get; set; }

    /// <summary>
    /// Formatted load (e.g., "135/95 lb").
    /// </summary>
    /// <example>135/95 lb</example>
    public string? LoadFormatted { get; set; }

    /// <summary>
    /// Distance amount.
    /// </summary>
    /// <example>400</example>
    public decimal? DistanceValue { get; set; }

    /// <summary>
    /// Distance unit.
    /// </summary>
    /// <example>M</example>
    public string? DistanceUnit { get; set; }

    /// <summary>
    /// Formatted distance (e.g., "400 m").
    /// </summary>
    /// <example>400 m</example>
    public string? DistanceFormatted { get; set; }

    /// <summary>
    /// Calorie target (male standard if gender-differentiated).
    /// </summary>
    /// <example>15</example>
    public int? Calories { get; set; }

    /// <summary>
    /// Calorie target for female athletes (if gender-differentiated).
    /// </summary>
    /// <example>12</example>
    public int? CaloriesFemale { get; set; }

    /// <summary>
    /// Time duration in seconds (for holds, etc.).
    /// </summary>
    /// <example>30</example>
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// Formatted duration (e.g., "0:30").
    /// </summary>
    /// <example>0:30</example>
    public string? DurationFormatted { get; set; }

    /// <summary>
    /// Additional notes or specifications.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Rep scheme reps for this movement (e.g., [21, 15, 9] for "21-15-9").
    /// Applied when movement has no explicit rep count but workout has a rep scheme.
    /// </summary>
    /// <example>[21, 15, 9]</example>
    public int[]? RepSchemeReps { get; set; }

    /// <summary>
    /// Type of rep scheme (Descending, Ascending, Fixed, Custom).
    /// </summary>
    /// <example>Descending</example>
    public string? RepSchemeType { get; set; }
}
