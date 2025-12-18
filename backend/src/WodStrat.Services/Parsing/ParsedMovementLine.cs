namespace WodStrat.Services.Parsing;

/// <summary>
/// Result of parsing a movement line with all extracted components.
/// </summary>
/// <param name="Reps">Extracted rep count if present.</param>
/// <param name="MovementText">The movement name/description (cleaned).</param>
/// <param name="Weight">Extracted weight if present.</param>
/// <param name="WeightPair">Extracted RX/Scaled weights if present.</param>
/// <param name="Distance">Extracted distance if present.</param>
/// <param name="Calories">Extracted calories if present.</param>
/// <param name="CaloriePair">Extracted RX/Scaled calories if present.</param>
/// <param name="DurationSeconds">Extracted duration in seconds if present.</param>
/// <param name="Height">Extracted height specification (e.g., "24 in" for box jumps).</param>
/// <param name="Modifiers">Additional modifiers text (parenthetical content).</param>
/// <param name="OriginalText">The original line text.</param>
public sealed record ParsedMovementLine(
    int? Reps,
    string MovementText,
    Weight? Weight,
    WeightPair? WeightPair,
    Distance? Distance,
    int? Calories,
    CaloriePair? CaloriePair,
    int? DurationSeconds,
    string? Height,
    string? Modifiers,
    string OriginalText
)
{
    /// <summary>
    /// Indicates if any quantity was extracted (reps, distance, calories, or duration).
    /// </summary>
    public bool HasQuantity => Reps.HasValue || Distance is not null || Calories.HasValue || DurationSeconds.HasValue;

    /// <summary>
    /// Indicates if the movement has load specification.
    /// </summary>
    public bool HasLoad => Weight is not null || WeightPair is not null;
}
