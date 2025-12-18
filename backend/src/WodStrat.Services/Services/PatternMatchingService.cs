using System.Globalization;
using System.Text.RegularExpressions;
using WodStrat.Dal.Enums;
using WodStrat.Services.Interfaces;
using WodStrat.Services.Parsing;

namespace WodStrat.Services.Services;

/// <summary>
/// Service implementation for regex-based pattern matching in workout text.
/// Provides stateless extraction methods for workout components.
/// </summary>
public class PatternMatchingService : IPatternMatchingService
{
    /// <inheritdoc />
    public WorkoutTypeMatch DetectWorkoutType(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new WorkoutTypeMatch(WorkoutType.ForTime, Confidence: 0.5);
        }

        // Check for Tabata first (most specific)
        var tabataMatch = WorkoutPatterns.TabataPattern().Match(text);
        if (tabataMatch.Success)
        {
            return new WorkoutTypeMatch(
                WorkoutType.Intervals,
                TimeCapSeconds: 4 * 60, // Standard Tabata is 4 minutes
                IntervalSeconds: 30, // 20 on / 10 off
                Confidence: 1.0,
                MatchedPattern: tabataMatch.Value
            );
        }

        // Check for AMRAP
        var amrapMatch = WorkoutPatterns.AmrapPattern().Match(text);
        if (amrapMatch.Success)
        {
            int? duration = null;
            if (amrapMatch.Groups[1].Success && int.TryParse(amrapMatch.Groups[1].Value, out var d1))
                duration = d1 * 60;
            else if (amrapMatch.Groups[2].Success && int.TryParse(amrapMatch.Groups[2].Value, out var d2))
                duration = d2 * 60;

            return new WorkoutTypeMatch(
                WorkoutType.Amrap,
                TimeCapSeconds: duration,
                Confidence: 1.0,
                MatchedPattern: amrapMatch.Value
            );
        }

        // Check for EMOM
        var emomMatch = WorkoutPatterns.EmomPattern().Match(text);
        if (emomMatch.Success)
        {
            int intervalMinutes = 1; // Default 1 minute
            if (emomMatch.Groups[1].Success && int.TryParse(emomMatch.Groups[1].Value, out var i1) && i1 > 0)
                intervalMinutes = i1;
            else if (emomMatch.Groups[2].Success && int.TryParse(emomMatch.Groups[2].Value, out var i2) && i2 > 0)
                intervalMinutes = i2;

            int? totalDuration = null;
            if (emomMatch.Groups[3].Success && int.TryParse(emomMatch.Groups[3].Value, out var d3))
                totalDuration = d3 * 60;
            else if (emomMatch.Groups[4].Success && int.TryParse(emomMatch.Groups[4].Value, out var d4))
                totalDuration = d4 * 60;

            return new WorkoutTypeMatch(
                WorkoutType.Emom,
                TimeCapSeconds: totalDuration,
                IntervalSeconds: intervalMinutes * 60,
                Confidence: 1.0,
                MatchedPattern: emomMatch.Value
            );
        }

        // Check for Interval patterns
        var intervalMatch = WorkoutPatterns.IntervalPattern().Match(text);
        if (intervalMatch.Success)
        {
            var config = ExtractInterval(text);
            if (config != null)
            {
                return new WorkoutTypeMatch(
                    WorkoutType.Intervals,
                    TimeCapSeconds: config.TotalSeconds,
                    RoundCount: config.Rounds,
                    IntervalSeconds: config.WorkSeconds + config.RestSeconds,
                    Confidence: 1.0,
                    MatchedPattern: intervalMatch.Value
                );
            }
        }

        // Check for For Time
        var forTimeMatch = WorkoutPatterns.ForTimePattern().Match(text);
        if (forTimeMatch.Success)
        {
            int? rounds = null;
            if (forTimeMatch.Groups[1].Success && int.TryParse(forTimeMatch.Groups[1].Value, out var r))
                rounds = r;

            var timeCap = ExtractTimeCap(text);

            return new WorkoutTypeMatch(
                WorkoutType.ForTime,
                TimeCapSeconds: timeCap.HasValue ? (int)timeCap.Value.TotalSeconds : null,
                RoundCount: rounds,
                Confidence: 1.0,
                MatchedPattern: forTimeMatch.Value
            );
        }

        // Check for Rounds (without "For Time")
        var roundsMatch = WorkoutPatterns.RoundsPattern().Match(text);
        if (roundsMatch.Success)
        {
            var rounds = int.Parse(roundsMatch.Groups[1].Value);
            return new WorkoutTypeMatch(
                WorkoutType.Rounds,
                RoundCount: rounds,
                Confidence: 0.9,
                MatchedPattern: roundsMatch.Value
            );
        }

        // Check for chipper rep scheme (implies For Time)
        var chipperMatch = WorkoutPatterns.ChipperRepSchemePattern().Match(text);
        if (chipperMatch.Success)
        {
            return new WorkoutTypeMatch(
                WorkoutType.ForTime,
                Confidence: 0.8,
                MatchedPattern: chipperMatch.Value
            );
        }

        // Default to ForTime with lower confidence
        return new WorkoutTypeMatch(WorkoutType.ForTime, Confidence: 0.5);
    }

    /// <inheritdoc />
    public RepScheme? ExtractRepScheme(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        // Try chipper pattern first: "21-15-9"
        var chipperMatch = WorkoutPatterns.ChipperRepSchemePattern().Match(text);
        if (chipperMatch.Success)
        {
            var repsStr = chipperMatch.Groups[1].Value;
            var reps = repsStr.Split('-').Select(int.Parse).ToList();
            var type = DetermineRepSchemeType(reps);
            return new RepScheme(reps, type, chipperMatch.Value);
        }

        // Try per-round pattern: "10/8/6/4/2"
        var perRoundMatch = WorkoutPatterns.PerRoundRepPattern().Match(text);
        if (perRoundMatch.Success)
        {
            var repsStr = perRoundMatch.Groups[1].Value;
            var reps = repsStr.Split('/').Select(int.Parse).ToList();
            var type = DetermineRepSchemeType(reps);
            return new RepScheme(reps, type, perRoundMatch.Value);
        }

        // Try fixed rep pattern: "5 rounds of 10 reps"
        var fixedMatch = WorkoutPatterns.FixedRepPattern().Match(text);
        if (fixedMatch.Success)
        {
            var rounds = int.Parse(fixedMatch.Groups[1].Value);
            var repsPerRound = int.Parse(fixedMatch.Groups[2].Value);
            var reps = Enumerable.Repeat(repsPerRound, rounds).ToList();
            return new RepScheme(reps, RepSchemeType.Fixed, fixedMatch.Value);
        }

        return null;
    }

    /// <inheritdoc />
    public ParsedMovementLine ParseMovementLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return new ParsedMovementLine(null, string.Empty, null, null, null, null, null, null, null, null, line);
        }

        var trimmedLine = line.Trim();
        int? reps = null;
        string movementText = trimmedLine;
        string? modifiers = null;
        int? durationSeconds = null;

        // Try movement with reps pattern first: "10 Pull-ups"
        var repsMatch = WorkoutPatterns.MovementWithRepsPattern().Match(trimmedLine);
        if (repsMatch.Success)
        {
            reps = int.Parse(repsMatch.Groups[1].Value);
            movementText = repsMatch.Groups[2].Value.Trim();
            modifiers = repsMatch.Groups[3].Success ? repsMatch.Groups[3].Value : null;
        }

        // Try movement with duration: "30 sec L-sit"
        var durationMatch = WorkoutPatterns.MovementWithDurationPattern().Match(trimmedLine);
        if (durationMatch.Success)
        {
            if (durationMatch.Groups[1].Success)
            {
                var value = int.Parse(durationMatch.Groups[1].Value);
                var unit = durationMatch.Groups[2].Value.ToLowerInvariant();
                durationSeconds = unit.StartsWith("min") ? value * 60 : value;
                movementText = durationMatch.Groups[3].Value.Trim();
            }
            else if (durationMatch.Groups[4].Success)
            {
                durationSeconds = int.Parse(durationMatch.Groups[4].Value);
                movementText = durationMatch.Groups[5].Value.Trim();
            }
        }

        // Extract components
        var weight = ExtractWeight(trimmedLine);
        var weightPair = ExtractWeightPair(trimmedLine);
        var distance = ExtractDistance(trimmedLine);
        var calories = ExtractCalories(trimmedLine);
        var caloriePair = ExtractCaloriePair(trimmedLine);
        var height = ExtractHeight(trimmedLine);

        // Clean up movement text by removing extracted components
        movementText = CleanMovementText(movementText, weightPair, weight, distance, caloriePair, calories);

        return new ParsedMovementLine(
            Reps: reps,
            MovementText: movementText,
            Weight: weightPair == null ? weight : null, // Prefer pair over single
            WeightPair: weightPair,
            Distance: distance,
            Calories: caloriePair == null ? calories : null, // Prefer pair over single
            CaloriePair: caloriePair,
            DurationSeconds: durationSeconds,
            Height: height,
            Modifiers: modifiers,
            OriginalText: line
        );
    }

    /// <inheritdoc />
    public Weight? ExtractWeight(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var match = WorkoutPatterns.LoadPattern().Match(text);
        if (!match.Success)
            return null;

        var value = decimal.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        var unitStr = match.Groups[2].Value.ToLowerInvariant();
        var unit = ParseLoadUnit(unitStr);

        return new Weight(value, unit, match.Value);
    }

    /// <inheritdoc />
    public WeightPair? ExtractWeightPair(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var match = WorkoutPatterns.GenderLoadPattern().Match(text);
        if (!match.Success)
            return null;

        var maleValue = decimal.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        var femaleValue = decimal.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
        var unit = match.Groups[3].Success ? ParseLoadUnit(match.Groups[3].Value) : LoadUnit.Lb;

        return new WeightPair(
            new Weight(maleValue, unit, $"{maleValue} {unit}"),
            new Weight(femaleValue, unit, $"{femaleValue} {unit}"),
            match.Value
        );
    }

    /// <inheritdoc />
    public PercentageLoad? ExtractPercentage(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var match = WorkoutPatterns.PercentagePattern().Match(text);
        if (!match.Success)
            return null;

        // Handle "bodyweight" / "BW" without percentage
        if (match.Value.Equals("bodyweight", StringComparison.OrdinalIgnoreCase) ||
            match.Value.Equals("BW", StringComparison.OrdinalIgnoreCase))
        {
            return new PercentageLoad(100, "bodyweight", match.Value);
        }

        if (!match.Groups[1].Success)
            return null;

        var percentage = int.Parse(match.Groups[1].Value);
        var reference = match.Groups[2].Success ? match.Groups[2].Value : null;

        return new PercentageLoad(percentage, reference, match.Value);
    }

    /// <inheritdoc />
    public Distance? ExtractDistance(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var match = WorkoutPatterns.DistancePattern().Match(text);
        if (!match.Success)
            return null;

        var value = decimal.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        var unitStr = match.Groups[2].Value.ToLowerInvariant();
        var unit = ParseDistanceUnit(unitStr);

        return new Distance(value, unit, match.Value);
    }

    /// <inheritdoc />
    public TimeSpan? ExtractTime(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        // Try clock format first: "12:00"
        var clockMatch = WorkoutPatterns.ClockFormatPattern().Match(text);
        if (clockMatch.Success)
        {
            var minutes = int.Parse(clockMatch.Groups[1].Value);
            var seconds = int.Parse(clockMatch.Groups[2].Value);
            return TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);
        }

        // Try duration pattern: "20 min", "30 sec", ":30"
        var durationMatch = WorkoutPatterns.DurationPattern().Match(text);
        if (durationMatch.Success)
        {
            if (durationMatch.Groups[3].Success) // :XX format
            {
                return TimeSpan.FromSeconds(int.Parse(durationMatch.Groups[3].Value));
            }

            var value = int.Parse(durationMatch.Groups[1].Value);
            var unit = durationMatch.Groups[2].Value.ToLowerInvariant();

            return unit.StartsWith("min")
                ? TimeSpan.FromMinutes(value)
                : TimeSpan.FromSeconds(value);
        }

        return null;
    }

    /// <inheritdoc />
    public TimeSpan? ExtractTimeCap(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var match = WorkoutPatterns.TimeCapPattern().Match(text);
        if (!match.Success)
            return null;

        // Handle "XX min cap" format (Group 3)
        if (match.Groups[3].Success)
        {
            var minutes = int.Parse(match.Groups[3].Value);
            return TimeSpan.FromMinutes(minutes);
        }

        // Handle "TC: XX" or "Time Cap: XX:YY" format
        var mins = int.Parse(match.Groups[1].Value);
        var secs = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;

        return TimeSpan.FromMinutes(mins) + TimeSpan.FromSeconds(secs);
    }

    /// <inheritdoc />
    public int? ExtractCalories(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var match = WorkoutPatterns.CaloriePattern().Match(text);
        if (!match.Success)
            return null;

        return int.Parse(match.Groups[1].Value);
    }

    /// <inheritdoc />
    public CaloriePair? ExtractCaloriePair(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var match = WorkoutPatterns.GenderCaloriePattern().Match(text);
        if (!match.Success)
            return null;

        var male = int.Parse(match.Groups[1].Value);
        var female = int.Parse(match.Groups[2].Value);

        return new CaloriePair(male, female, match.Value);
    }

    /// <inheritdoc />
    public int? ExtractRounds(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var match = WorkoutPatterns.RoundsPattern().Match(text);
        if (!match.Success)
            return null;

        return int.Parse(match.Groups[1].Value);
    }

    /// <inheritdoc />
    public IntervalConfig? ExtractInterval(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        // Check for Tabata pattern first
        if (WorkoutPatterns.TabataPattern().IsMatch(text))
        {
            return new IntervalConfig(8, 20, 10, "Tabata");
        }

        var match = WorkoutPatterns.IntervalPattern().Match(text);
        if (!match.Success)
            return null;

        var rounds = int.Parse(match.Groups[1].Value);

        // Handle "5 x 500m" style (distance-based, no work/rest split)
        if (match.Groups[6].Success)
        {
            return null; // Distance-based intervals handled differently
        }

        // Parse work time
        var workSeconds = 0;
        if (match.Groups[2].Success)
        {
            var workValue = int.Parse(match.Groups[2].Value);
            var workUnit = match.Groups[3].Success ? match.Groups[3].Value : "min";
            workSeconds = workUnit.StartsWith("min", StringComparison.OrdinalIgnoreCase)
                ? workValue * 60
                : workValue;
        }

        // Parse rest time
        var restSeconds = 0;
        if (match.Groups[4].Success)
        {
            var restValue = int.Parse(match.Groups[4].Value);
            var restUnit = match.Groups[5].Success ? match.Groups[5].Value : "min";
            restSeconds = restUnit.StartsWith("min", StringComparison.OrdinalIgnoreCase)
                ? restValue * 60
                : restValue;
        }

        return new IntervalConfig(rounds, workSeconds, restSeconds, match.Value);
    }

    #region Private Helper Methods

    private static RepSchemeType DetermineRepSchemeType(IReadOnlyList<int> reps)
    {
        if (reps.Count <= 1)
            return RepSchemeType.Fixed;

        var isDescending = true;
        var isAscending = true;
        var isFixed = true;

        for (var i = 1; i < reps.Count; i++)
        {
            if (reps[i] >= reps[i - 1]) isDescending = false;
            if (reps[i] <= reps[i - 1]) isAscending = false;
            if (reps[i] != reps[0]) isFixed = false;
        }

        if (isFixed) return RepSchemeType.Fixed;
        if (isDescending) return RepSchemeType.Descending;
        if (isAscending) return RepSchemeType.Ascending;
        return RepSchemeType.Custom;
    }

    private static LoadUnit ParseLoadUnit(string unit)
    {
        return unit.ToLowerInvariant() switch
        {
            "kg" => LoadUnit.Kg,
            "lb" or "lbs" or "#" => LoadUnit.Lb,
            "pood" or "poods" => LoadUnit.Pood,
            _ => LoadUnit.Lb
        };
    }

    private static DistanceUnit ParseDistanceUnit(string unit)
    {
        return unit.ToLowerInvariant() switch
        {
            "m" or "meter" or "meters" => DistanceUnit.M,
            "km" or "k" => DistanceUnit.Km,
            "ft" or "feet" => DistanceUnit.Ft,
            "mi" or "mile" or "miles" => DistanceUnit.Mi,
            _ => DistanceUnit.M
        };
    }

    private static string? ExtractHeight(string text)
    {
        var match = WorkoutPatterns.HeightPattern().Match(text);
        return match.Success ? match.Value : null;
    }

    private static string CleanMovementText(
        string text,
        WeightPair? weightPair,
        Weight? weight,
        Distance? distance,
        CaloriePair? caloriePair,
        int? calories)
    {
        var result = text;

        // Remove weight specifications
        if (weightPair != null)
            result = result.Replace(weightPair.OriginalText, "");
        else if (weight != null)
            result = result.Replace(weight.OriginalText, "");

        // Remove distance
        if (distance != null)
            result = result.Replace(distance.OriginalText, "");

        // Remove calories
        if (caloriePair != null)
            result = result.Replace(caloriePair.OriginalText, "");
        else if (calories.HasValue)
        {
            result = WorkoutPatterns.CaloriePattern().Replace(result, "");
        }

        // Remove empty parentheses and clean up
        result = Regex.Replace(result, @"\(\s*\)", "");
        result = Regex.Replace(result, @"\s+", " ");

        return result.Trim();
    }

    #endregion
}
