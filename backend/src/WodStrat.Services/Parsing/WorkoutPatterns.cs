using System.Text.RegularExpressions;

namespace WodStrat.Services.Parsing;

/// <summary>
/// Static class containing all compiled regex patterns for workout text parsing.
/// Uses source-generated regex for optimal performance and AOT compatibility.
/// </summary>
public static partial class WorkoutPatterns
{
    #region Workout Type Patterns

    /// <summary>
    /// AMRAP patterns: "20 min AMRAP", "AMRAP 20", "15 minute AMRAP", "AMRAP in 20 minutes"
    /// Groups: 1=duration before AMRAP, 2=duration after AMRAP/in
    /// </summary>
    [GeneratedRegex(@"(?:(\d+)\s*(?:min(?:ute)?s?)\s*AMRAP|AMRAP\s*(?:in\s*)?(\d+)?\s*(?:min(?:ute)?s?)?|AMRAP)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex AmrapPattern();

    /// <summary>
    /// For Time patterns: "For Time", "3 Rounds For Time", "Complete as fast as possible"
    /// Groups: 1=round count if present
    /// </summary>
    [GeneratedRegex(@"(?:(\d+)\s*(?:Rounds?\s*)?For\s*Time|For\s*Time|Complete\s+as\s+fast\s+as\s+possible)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex ForTimePattern();

    /// <summary>
    /// EMOM patterns: "EMOM", "E2MOM", "E3MOM", "Every 2 minutes", "EMOM 20", "10 min EMOM"
    /// Groups: 1=interval minutes (E#MOM), 2=interval minutes (Every # min), 3=duration before EMOM, 4=duration after EMOM
    /// </summary>
    [GeneratedRegex(@"(?:E(\d*)MOM|Every\s*(\d+)?\s*(?:min(?:ute)?s?)\s*(?:on\s*the\s*(?:min(?:ute)?)?)?|(\d+)\s*(?:min(?:ute)?s?)\s*EMOM|EMOM\s*(\d+)?)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex EmomPattern();

    /// <summary>
    /// Rounds patterns: "5 Rounds", "5 RFT", "3 Sets", "5 rounds of"
    /// Groups: 1=round count
    /// </summary>
    [GeneratedRegex(@"(\d+)\s*(?:Rounds?|RFT|Sets?)\s*(?:of|:)?", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex RoundsPattern();

    /// <summary>
    /// Interval patterns: "5 x 3 min on / 1 min off", "8 rounds of :20 on/:10 off", "4 x 500m"
    /// Groups: 1=rounds, 2=work time, 3=work unit, 4=rest time, 5=rest unit
    /// </summary>
    [GeneratedRegex(@"(\d+)\s*[xX]\s*(?:(\d+)\s*(min|sec|:?\d+)?\s*(?:on|work)?\s*/?\s*(\d+)?\s*(min|sec)?\s*(?:off|rest)?|(\d+)\s*[mM])", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex IntervalPattern();

    /// <summary>
    /// Tabata patterns: "Tabata", "8 rounds of :20 on/:10 off", "Tabata intervals"
    /// </summary>
    [GeneratedRegex(@"Tabata|8\s*(?:rounds?\s*(?:of\s*)?)?:?20\s*(?:sec(?:ond)?s?)?\s*(?:on|work)?\s*[/:]?\s*:?10\s*(?:sec(?:ond)?s?)?\s*(?:off|rest)?", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex TabataPattern();

    #endregion

    #region Time Patterns

    /// <summary>
    /// Time cap patterns: "Time Cap: 20", "20 min cap", "Cap: 15:00", "TC: 12", "15 minute cap"
    /// Groups: 1=minutes, 2=seconds (for MM:SS format), 3=minutes (for "XX min cap" format)
    /// </summary>
    [GeneratedRegex(@"(?:Time\s*Cap|Cap|TC)\s*[:=]?\s*(\d+)(?::(\d+))?(?:\s*(?:min(?:ute)?s?)?)?|(\d+)\s*(?:min(?:ute)?s?)\s*cap", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex TimeCapPattern();

    /// <summary>
    /// Duration patterns: "20 min", "20 minutes", "30 sec", "30 seconds", ":30"
    /// Groups: 1=value, 2=unit (min/sec), or 3=seconds only (for :XX format)
    /// </summary>
    [GeneratedRegex(@"(?:(\d+)\s*(min(?:ute)?s?|sec(?:ond)?s?)|:(\d+))", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex DurationPattern();

    /// <summary>
    /// Clock format patterns: "12:00", "1:30", "0:45"
    /// Groups: 1=minutes, 2=seconds
    /// </summary>
    [GeneratedRegex(@"(\d{1,2}):(\d{2})(?!\d)", RegexOptions.Compiled)]
    public static partial Regex ClockFormatPattern();

    #endregion

    #region Rep Scheme Patterns

    /// <summary>
    /// Chipper rep scheme: "21-15-9", "10-9-8-7-6-5-4-3-2-1", "50-40-30-20-10"
    /// Groups: 1=full match
    /// </summary>
    [GeneratedRegex(@"^(\d+(?:-\d+)+)$", RegexOptions.Multiline | RegexOptions.Compiled)]
    public static partial Regex ChipperRepSchemePattern();

    /// <summary>
    /// Fixed rep pattern: "5 rounds of 10 reps", "3 sets of 12"
    /// Groups: 1=rounds, 2=reps
    /// </summary>
    [GeneratedRegex(@"(\d+)\s*(?:rounds?|sets?)\s*(?:of\s*)?(\d+)\s*(?:reps?)?", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex FixedRepPattern();

    /// <summary>
    /// Per-round rep pattern: "10/8/6/4/2"
    /// Groups: 1=full sequence
    /// </summary>
    [GeneratedRegex(@"(\d+(?:/\d+)+)", RegexOptions.Compiled)]
    public static partial Regex PerRoundRepPattern();

    #endregion

    #region Movement Line Patterns

    /// <summary>
    /// Movement with reps: "10 Pull-ups", "21 Thrusters", "15 Box Jumps (24 in)"
    /// Groups: 1=reps, 2=movement name, 3=modifiers in parentheses
    /// </summary>
    [GeneratedRegex(@"^(\d+)\s+(.+?)(?:\s*\(([^)]+)\))?$", RegexOptions.Multiline | RegexOptions.Compiled)]
    public static partial Regex MovementWithRepsPattern();

    /// <summary>
    /// Movement with duration: "30 sec L-sit", ":30 Plank Hold", "1 min Hollow Hold"
    /// Groups: 1=duration value, 2=unit, 3=movement name, or 4=seconds (for :XX), 5=movement
    /// </summary>
    [GeneratedRegex(@"(?:(\d+)\s*(min(?:ute)?s?|sec(?:ond)?s?)\s+(.+)|:(\d+)\s+(.+))", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex MovementWithDurationPattern();

    #endregion

    #region Weight Patterns

    /// <summary>
    /// Load pattern (single weight): "135 lb", "60 kg", "1.5 pood", "95#", "95 lbs"
    /// Groups: 1=value, 2=unit
    /// </summary>
    [GeneratedRegex(@"(\d+(?:\.\d+)?)\s*(lb|lbs?|kg|pood|#)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex LoadPattern();

    /// <summary>
    /// Gender load pattern (RX/Scaled): "(135/95 lb)", "(95/65)", "95/65 lb"
    /// Groups: 1=male weight, 2=female weight, 3=unit (optional)
    /// </summary>
    [GeneratedRegex(@"(\d+(?:\.\d+)?)\s*/\s*(\d+(?:\.\d+)?)\s*(lb|lbs?|kg|pood)?", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex GenderLoadPattern();

    /// <summary>
    /// Percentage load pattern: "70%", "70% 1RM", "85% of 1RM", "bodyweight"
    /// Groups: 1=percentage, 2=reference (optional)
    /// </summary>
    [GeneratedRegex(@"(\d+)\s*%\s*(?:of\s*)?(1RM|bodyweight|BW)?|bodyweight|BW", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex PercentagePattern();

    #endregion

    #region Distance Patterns

    /// <summary>
    /// Distance pattern: "400m", "400 meters", "5k", "5 km", "100 ft", "1 mile", "1 mi"
    /// Groups: 1=value, 2=unit
    /// </summary>
    [GeneratedRegex(@"(\d+(?:\.\d+)?)\s*(m(?:eters?)?|km|k|ft|feet|mi(?:les?)?)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex DistancePattern();

    #endregion

    #region Calorie Patterns

    /// <summary>
    /// Calorie pattern (single): "20 Cal Row", "20 calories", "15 cal bike"
    /// Groups: 1=calories
    /// </summary>
    [GeneratedRegex(@"(\d+)\s*(?:Cal(?:orie)?s?)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex CaloriePattern();

    /// <summary>
    /// Gender calorie pattern: "20/15 Cal", "25/20 calories"
    /// Groups: 1=male calories, 2=female calories
    /// </summary>
    [GeneratedRegex(@"(\d+)\s*/\s*(\d+)\s*(?:Cal(?:orie)?s?)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex GenderCaloriePattern();

    #endregion

    #region Height/Equipment Patterns

    /// <summary>
    /// Height pattern for box jumps, wall balls: "24 in", "20\"", "24-inch"
    /// Groups: 1=value
    /// </summary>
    [GeneratedRegex(@"(\d+)\s*(?:in(?:ch)?(?:es)?|""|\'\')", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex HeightPattern();

    #endregion

    #region Helper Methods

    /// <summary>
    /// Checks if text likely contains a workout type indicator.
    /// </summary>
    public static bool ContainsWorkoutType(string text)
    {
        return AmrapPattern().IsMatch(text) ||
               ForTimePattern().IsMatch(text) ||
               EmomPattern().IsMatch(text) ||
               TabataPattern().IsMatch(text) ||
               RoundsPattern().IsMatch(text);
    }

    /// <summary>
    /// Checks if text is a header line (workout type indicator, time cap, rep scheme).
    /// </summary>
    public static bool IsHeaderLine(string line)
    {
        var trimmed = line.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ||
               AmrapPattern().IsMatch(trimmed) ||
               ForTimePattern().IsMatch(trimmed) ||
               EmomPattern().IsMatch(trimmed) ||
               TabataPattern().IsMatch(trimmed) ||
               TimeCapPattern().IsMatch(trimmed) ||
               ChipperRepSchemePattern().IsMatch(trimmed);
    }

    #endregion
}
