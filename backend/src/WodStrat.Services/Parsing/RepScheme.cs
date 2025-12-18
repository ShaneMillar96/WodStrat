using WodStrat.Dal.Enums;

namespace WodStrat.Services.Parsing;

/// <summary>
/// Represents a rep scheme pattern (e.g., "21-15-9", "10-9-8-7-6-5-4-3-2-1").
/// </summary>
/// <param name="Reps">The sequence of rep counts in order.</param>
/// <param name="Type">The type of rep scheme (Descending, Ascending, Fixed, Custom).</param>
/// <param name="OriginalText">The original matched text.</param>
public sealed record RepScheme(
    IReadOnlyList<int> Reps,
    RepSchemeType Type,
    string OriginalText
)
{
    /// <summary>
    /// Total volume (sum of all reps).
    /// </summary>
    public int TotalReps => Reps.Sum();

    /// <summary>
    /// Number of rounds/sets in the scheme.
    /// </summary>
    public int RoundCount => Reps.Count;
}
