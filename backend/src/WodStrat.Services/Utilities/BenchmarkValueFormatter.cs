using WodStrat.Dal.Enums;

namespace WodStrat.Services.Utilities;

/// <summary>
/// Utility class for formatting benchmark values based on metric type.
/// </summary>
public static class BenchmarkValueFormatter
{
    /// <summary>
    /// Formats a benchmark value based on its metric type.
    /// </summary>
    /// <param name="value">The raw numeric value.</param>
    /// <param name="metricType">The metric type as string (Time, Reps, Weight, Pace).</param>
    /// <param name="unit">The display unit.</param>
    /// <returns>A human-readable formatted string.</returns>
    public static string Format(decimal value, string metricType, string unit)
    {
        if (Enum.TryParse<BenchmarkMetricType>(metricType, ignoreCase: true, out var parsedType))
        {
            return Format(value, parsedType, unit);
        }

        return value.ToString("0.##");
    }

    /// <summary>
    /// Formats a benchmark value based on its metric type.
    /// </summary>
    /// <param name="value">The raw numeric value.</param>
    /// <param name="metricType">The metric type enum value.</param>
    /// <param name="unit">The display unit.</param>
    /// <returns>A human-readable formatted string.</returns>
    public static string Format(decimal value, BenchmarkMetricType metricType, string unit)
    {
        return metricType switch
        {
            BenchmarkMetricType.Time => FormatTime(value),
            BenchmarkMetricType.Reps => FormatReps(value),
            BenchmarkMetricType.Weight => FormatWeight(value, unit),
            BenchmarkMetricType.Pace => FormatPace(value, unit),
            _ => value.ToString("0.##")
        };
    }

    /// <summary>
    /// Formats a time value (in seconds) to mm:ss or hh:mm:ss format.
    /// </summary>
    /// <param name="totalSeconds">Total seconds.</param>
    /// <returns>Formatted time string.</returns>
    public static string FormatTime(decimal totalSeconds)
    {
        var timeSpan = TimeSpan.FromSeconds((double)Math.Abs(totalSeconds));

        if (timeSpan.TotalHours >= 1)
        {
            return $"{(int)timeSpan.TotalHours}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }

        return $"{(int)timeSpan.TotalMinutes}:{timeSpan.Seconds:D2}";
    }

    /// <summary>
    /// Formats a rep count.
    /// </summary>
    /// <param name="reps">Number of repetitions.</param>
    /// <returns>Formatted string like "25 reps".</returns>
    public static string FormatReps(decimal reps)
    {
        var wholeReps = (int)Math.Round(reps);
        return $"{wholeReps} reps";
    }

    /// <summary>
    /// Formats a weight value.
    /// </summary>
    /// <param name="weight">Weight value.</param>
    /// <param name="unit">Unit (defaults to "kg").</param>
    /// <returns>Formatted string like "100.5 kg".</returns>
    public static string FormatWeight(decimal weight, string unit)
    {
        var displayUnit = string.IsNullOrWhiteSpace(unit) ? "kg" : unit;

        // Show decimal only if needed
        if (weight == Math.Floor(weight))
        {
            return $"{(int)weight} {displayUnit}";
        }

        return $"{weight:0.#} {displayUnit}";
    }

    /// <summary>
    /// Formats a pace value (seconds per unit) to m:ss/unit format.
    /// </summary>
    /// <param name="secondsPerUnit">Seconds per distance unit.</param>
    /// <param name="unit">Distance unit (e.g., "500m").</param>
    /// <returns>Formatted string like "1:45/500m".</returns>
    public static string FormatPace(decimal secondsPerUnit, string unit)
    {
        var timeSpan = TimeSpan.FromSeconds((double)Math.Abs(secondsPerUnit));
        var paceFormatted = $"{(int)timeSpan.TotalMinutes}:{timeSpan.Seconds:D2}";

        if (string.IsNullOrWhiteSpace(unit))
        {
            return paceFormatted;
        }

        return $"{paceFormatted}/{unit}";
    }
}
