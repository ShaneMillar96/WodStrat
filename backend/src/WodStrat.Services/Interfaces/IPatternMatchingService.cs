using WodStrat.Services.Parsing;

namespace WodStrat.Services.Interfaces;

/// <summary>
/// Service interface for regex-based pattern matching in workout text.
/// Provides extraction methods for workout types, rep schemes, movements, and measurements.
/// </summary>
public interface IPatternMatchingService
{
    /// <summary>
    /// Detects the workout type from text (AMRAP, For Time, EMOM, Rounds, Intervals, Tabata).
    /// </summary>
    /// <param name="text">The workout text to analyze.</param>
    /// <returns>The detected workout type with confidence metadata.</returns>
    WorkoutTypeMatch DetectWorkoutType(string text);

    /// <summary>
    /// Extracts rep scheme from workout text (e.g., "21-15-9", "10-9-8-7-6-5-4-3-2-1").
    /// </summary>
    /// <param name="text">The workout text containing rep scheme.</param>
    /// <returns>The parsed rep scheme if found; otherwise null.</returns>
    RepScheme? ExtractRepScheme(string text);

    /// <summary>
    /// Parses a single movement line to extract reps, movement name, and modifiers.
    /// </summary>
    /// <param name="line">A single line of movement text (e.g., "21 Thrusters (95/65 lb)").</param>
    /// <returns>Parsed movement data with extracted components.</returns>
    ParsedMovementLine ParseMovementLine(string line);

    /// <summary>
    /// Extracts weight/load specification from text.
    /// </summary>
    /// <param name="text">Text containing weight specification (e.g., "95 lb", "43 kg", "1.5 pood").</param>
    /// <returns>The parsed weight if found; otherwise null.</returns>
    Weight? ExtractWeight(string text);

    /// <summary>
    /// Extracts RX/Scaled weight specification (gender-differentiated).
    /// </summary>
    /// <param name="text">Text containing RX/Scaled weights (e.g., "(95/65 lb)", "(135/95)").</param>
    /// <returns>The parsed weight pair if found; otherwise null.</returns>
    WeightPair? ExtractWeightPair(string text);

    /// <summary>
    /// Extracts percentage-based load from text.
    /// </summary>
    /// <param name="text">Text containing percentage (e.g., "70%", "70% 1RM").</param>
    /// <returns>The parsed percentage if found; otherwise null.</returns>
    PercentageLoad? ExtractPercentage(string text);

    /// <summary>
    /// Extracts distance specification from text.
    /// </summary>
    /// <param name="text">Text containing distance (e.g., "400m", "5k", "1 mile").</param>
    /// <returns>The parsed distance if found; otherwise null.</returns>
    Distance? ExtractDistance(string text);

    /// <summary>
    /// Extracts time/duration from text.
    /// </summary>
    /// <param name="text">Text containing time specification (e.g., "20 min", "30 sec", "12:00").</param>
    /// <returns>The parsed time span if found; otherwise null.</returns>
    TimeSpan? ExtractTime(string text);

    /// <summary>
    /// Extracts time cap from workout text.
    /// </summary>
    /// <param name="text">Text containing time cap (e.g., "TC: 15 min", "15 min cap").</param>
    /// <returns>The parsed time cap if found; otherwise null.</returns>
    TimeSpan? ExtractTimeCap(string text);

    /// <summary>
    /// Extracts calorie target from text.
    /// </summary>
    /// <param name="text">Text containing calories (e.g., "20 Cal Row", "20/15 Cal").</param>
    /// <returns>The parsed calories if found; otherwise null.</returns>
    int? ExtractCalories(string text);

    /// <summary>
    /// Extracts gender-differentiated calorie targets.
    /// </summary>
    /// <param name="text">Text containing RX/Scaled calories (e.g., "20/15 Cal").</param>
    /// <returns>The parsed calorie pair if found; otherwise null.</returns>
    CaloriePair? ExtractCaloriePair(string text);

    /// <summary>
    /// Extracts round count from workout text.
    /// </summary>
    /// <param name="text">Text containing round specification (e.g., "5 rounds", "3 RFT").</param>
    /// <returns>The number of rounds if found; otherwise null.</returns>
    int? ExtractRounds(string text);

    /// <summary>
    /// Extracts interval configuration from text.
    /// </summary>
    /// <param name="text">Text containing interval spec (e.g., "5 x 3 min on / 1 min off").</param>
    /// <returns>The parsed interval configuration if found; otherwise null.</returns>
    IntervalConfig? ExtractInterval(string text);
}
