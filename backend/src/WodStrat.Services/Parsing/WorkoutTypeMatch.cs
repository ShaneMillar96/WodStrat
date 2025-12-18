using WodStrat.Dal.Enums;

namespace WodStrat.Services.Parsing;

/// <summary>
/// Result of workout type detection with metadata.
/// </summary>
/// <param name="Type">The detected workout type.</param>
/// <param name="TimeCapSeconds">Time cap in seconds if detected.</param>
/// <param name="RoundCount">Number of rounds if detected.</param>
/// <param name="IntervalSeconds">Interval duration in seconds (for EMOM/Intervals).</param>
/// <param name="Confidence">Detection confidence (0-1 scale).</param>
/// <param name="MatchedPattern">The pattern text that matched.</param>
public sealed record WorkoutTypeMatch(
    WorkoutType Type,
    int? TimeCapSeconds = null,
    int? RoundCount = null,
    int? IntervalSeconds = null,
    double Confidence = 1.0,
    string? MatchedPattern = null
);
