using WodStrat.Dal.Enums;
using WodStrat.Dal.Models;

namespace WodStrat.Services.Utilities;

/// <summary>
/// Classification of cardio movement sub-types for pace calculation.
/// </summary>
public enum CardioType
{
    Running,
    Rowing,
    Other
}

/// <summary>
/// Utility class for calculating cardio pace targets from benchmark data.
/// Encapsulates all cardio pace derivation and adjustment logic.
/// </summary>
public static class CardioPaceCalculator
{
    /// <summary>
    /// Converts a running benchmark time to pace per km and pace per mile.
    /// </summary>
    /// <param name="benchmarkTimeSeconds">Athlete's benchmark time in seconds.</param>
    /// <param name="benchmarkDistanceMeters">Distance of the benchmark in meters (e.g., 5000 for 5k, 1609 for mile).</param>
    /// <returns>Tuple of (secondsPerKm, secondsPerMile).</returns>
    public static (decimal SecondsPerKm, decimal SecondsPerMile) CalculateRunningPace(
        decimal benchmarkTimeSeconds,
        decimal benchmarkDistanceMeters)
    {
        var secondsPerKm = benchmarkTimeSeconds / (benchmarkDistanceMeters / 1000m);
        var secondsPerMile = secondsPerKm * 1.60934m;

        return (secondsPerKm, secondsPerMile);
    }

    /// <summary>
    /// Converts a rowing benchmark to pace per 500m.
    /// </summary>
    /// <param name="benchmarkValue">Athlete's benchmark value (time in seconds, or pace in sec/500m).</param>
    /// <param name="benchmarkSlug">The benchmark slug to determine conversion method.</param>
    /// <returns>Seconds per 500m.</returns>
    public static decimal CalculateRowingPace(
        decimal benchmarkValue,
        string benchmarkSlug)
    {
        return benchmarkSlug switch
        {
            "500m-row" => benchmarkValue,           // Direct: value is time for 500m
            "2k-row" => benchmarkValue / 4m,        // Divide by 4: 2000m / 500m = 4 splits
            "1k-row-pace" => benchmarkValue,        // Direct: value is already sec/500m
            _ => benchmarkValue                      // Default: assume direct pace
        };
    }

    /// <summary>
    /// Applies workout context adjustment to a raw benchmark pace.
    /// Longer workouts require slower pacing; shorter allow pushing closer to benchmark.
    /// </summary>
    /// <param name="rawPaceSeconds">Raw pace in seconds per unit (from benchmark derivation).</param>
    /// <param name="pacingLevel">Athlete's determined pacing level.</param>
    /// <param name="workout">The workout context (nullable).</param>
    /// <param name="workoutMovement">The specific movement context (nullable).</param>
    /// <returns>Adjusted pace in seconds per unit.</returns>
    public static decimal ApplyContextAdjustment(
        decimal rawPaceSeconds,
        PacingLevel pacingLevel,
        Workout? workout,
        WorkoutMovement? workoutMovement)
    {
        // Step 1 - Determine workout context factor
        decimal contextFactor = 1.05m; // default: medium effort

        if (workout != null)
        {
            // Short sprint context
            bool isShortDistance = workoutMovement?.DistanceValue != null
                && ConvertToMeters(workoutMovement.DistanceValue.Value, workoutMovement.DistanceUnit) <= 400;
            bool isShortWorkout = workout.TimeCapSeconds != null && workout.TimeCapSeconds <= 600; // 10 min or less

            if (isShortDistance && isShortWorkout)
            {
                contextFactor = 1.00m; // Sprint effort
            }
            // Long AMRAP context (check before generic long workout)
            else if (workout.WorkoutType == WorkoutType.Amrap && workout.TimeCapSeconds != null && workout.TimeCapSeconds >= 1200)
            {
                contextFactor = 1.20m; // Long AMRAP
            }
            // Long workout context
            else if (workout.TimeCapSeconds != null && workout.TimeCapSeconds >= 1200) // 20+ min
            {
                contextFactor = 1.15m; // Must conserve energy
            }
            else
            {
                contextFactor = 1.07m; // Medium effort (moderate workout)
            }
        }

        // Step 2 - Apply pacing level modifier
        decimal pacingModifier = pacingLevel switch
        {
            PacingLevel.Heavy => 0.97m,    // Push harder
            PacingLevel.Moderate => 1.00m,  // Steady pace
            PacingLevel.Light => 1.05m,     // Conservative
            _ => 1.00m
        };

        // Step 3 - Combine
        return rawPaceSeconds * contextFactor * pacingModifier;
    }

    /// <summary>
    /// Returns the distance in meters for a known benchmark slug.
    /// </summary>
    /// <param name="benchmarkSlug">Benchmark slug.</param>
    /// <returns>Distance in meters, or null if not a recognized running benchmark.</returns>
    public static decimal? GetBenchmarkDistanceMeters(string benchmarkSlug)
    {
        return benchmarkSlug switch
        {
            "5k-run" => 5000m,
            "1-mile-run" => 1609.34m,
            _ => null
        };
    }

    /// <summary>
    /// Determines the cardio sub-type from the movement's canonical name.
    /// </summary>
    /// <param name="canonicalName">The movement's canonical name.</param>
    /// <returns>The cardio type classification.</returns>
    public static CardioType DetermineCardioType(string canonicalName)
    {
        return canonicalName switch
        {
            "run" or "shuttle_run" => CardioType.Running,
            "row" => CardioType.Rowing,
            _ => CardioType.Other
        };
    }

    /// <summary>
    /// Converts a distance value with its unit to meters.
    /// </summary>
    /// <param name="value">The distance value.</param>
    /// <param name="unit">The distance unit (nullable, defaults to meters).</param>
    /// <returns>Distance in meters.</returns>
    public static decimal ConvertToMeters(decimal value, DistanceUnit? unit)
    {
        return unit switch
        {
            DistanceUnit.M => value,
            DistanceUnit.Km => value * 1000m,
            DistanceUnit.Mi => value * 1609.34m,
            DistanceUnit.Ft => value * 0.3048m,
            null => value, // assume meters
            _ => value
        };
    }

    /// <summary>
    /// Formats a pace value (seconds per unit) as a human-readable string (e.g., "4:35").
    /// </summary>
    /// <param name="secondsPerUnit">The pace in seconds per unit.</param>
    /// <returns>Formatted string in "m:ss" format.</returns>
    public static string FormatPace(decimal secondsPerUnit)
    {
        var timeSpan = TimeSpan.FromSeconds((double)Math.Abs(secondsPerUnit));
        return $"{(int)timeSpan.TotalMinutes}:{timeSpan.Seconds:D2}";
    }
}
