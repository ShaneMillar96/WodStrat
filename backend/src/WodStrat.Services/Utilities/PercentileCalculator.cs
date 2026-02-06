using WodStrat.Dal.Enums;
using WodStrat.Dal.Models;

namespace WodStrat.Services.Utilities;

/// <summary>
/// Centralized percentile calculation logic.
/// Extracts duplicate calculation logic from PacingService and VolumeLoadService.
/// </summary>
public static class PercentileCalculator
{
    /// <summary>
    /// Calculates athlete's percentile within population.
    /// Handles both "higher is better" and "lower is better" metrics.
    /// </summary>
    /// <param name="athleteValue">The athlete's benchmark value.</param>
    /// <param name="populationData">Population percentile reference data.</param>
    /// <param name="metricType">The metric type (determines if higher/lower is better).</param>
    /// <returns>Percentile value (0-100).</returns>
    public static decimal CalculatePercentile(
        decimal athleteValue,
        PopulationBenchmarkPercentile populationData,
        BenchmarkMetricType metricType)
    {
        // Define the percentile brackets from the population data
        var brackets = new (decimal percentile, decimal value)[]
        {
            (20m, populationData.Percentile20),
            (40m, populationData.Percentile40),
            (60m, populationData.Percentile60),
            (80m, populationData.Percentile80),
            (95m, populationData.Percentile95)
        };

        // For Time and Pace metrics, lower is better
        // For Reps and Weight metrics, higher is better
        bool lowerIsBetter = metricType == BenchmarkMetricType.Time ||
                            metricType == BenchmarkMetricType.Pace;

        if (lowerIsBetter)
        {
            return CalculatePercentileLowerIsBetter(athleteValue, brackets);
        }
        else
        {
            return CalculatePercentileHigherIsBetter(athleteValue, brackets);
        }
    }

    /// <summary>
    /// Calculates percentile for weight-based benchmarks (higher is better).
    /// Convenience method for when metric type is known to be weight-based.
    /// </summary>
    /// <param name="athleteValue">The athlete's benchmark value.</param>
    /// <param name="populationData">Population percentile reference data.</param>
    /// <returns>Percentile value (0-100).</returns>
    public static decimal CalculateWeightPercentile(
        decimal athleteValue,
        PopulationBenchmarkPercentile populationData)
    {
        return CalculatePercentile(athleteValue, populationData, BenchmarkMetricType.Weight);
    }

    /// <summary>
    /// Calculates percentile for time-based benchmarks (lower is better).
    /// Convenience method for when metric type is known to be time-based.
    /// </summary>
    /// <param name="athleteValue">The athlete's benchmark value.</param>
    /// <param name="populationData">Population percentile reference data.</param>
    /// <returns>Percentile value (0-100).</returns>
    public static decimal CalculateTimePercentile(
        decimal athleteValue,
        PopulationBenchmarkPercentile populationData)
    {
        return CalculatePercentile(athleteValue, populationData, BenchmarkMetricType.Time);
    }

    #region Private Helper Methods

    /// <summary>
    /// Calculates percentile for metrics where higher is better (Weight, Reps).
    /// </summary>
    private static decimal CalculatePercentileHigherIsBetter(
        decimal athleteValue,
        (decimal percentile, decimal value)[] brackets)
    {
        // Check if athlete is better than the best bracket
        if (athleteValue >= brackets[4].value)
        {
            var extraRange = brackets[4].value - brackets[3].value;
            if (extraRange <= 0) return 95m;
            return Math.Min(100m, 95m + (5m * (athleteValue - brackets[4].value) / extraRange));
        }

        // Check if athlete is worse than the lowest bracket
        if (athleteValue <= brackets[0].value)
        {
            if (brackets[0].value <= 0) return 0m;
            return Math.Max(0m, 20m * athleteValue / brackets[0].value);
        }

        // Find which bracket the athlete falls into and interpolate
        for (int i = 0; i < brackets.Length - 1; i++)
        {
            var lowerBracket = brackets[i];
            var higherBracket = brackets[i + 1];

            if (athleteValue >= lowerBracket.value && athleteValue <= higherBracket.value)
            {
                var range = higherBracket.value - lowerBracket.value;
                if (range <= 0) return lowerBracket.percentile;

                var position = athleteValue - lowerBracket.value;
                var percentileRange = higherBracket.percentile - lowerBracket.percentile;

                return lowerBracket.percentile + (percentileRange * position / range);
            }
        }

        // Fallback (shouldn't reach here)
        return 50m;
    }

    /// <summary>
    /// Calculates percentile for metrics where lower is better (Time, Pace).
    /// For lower-is-better metrics:
    /// - If athlete value is LOWER than 95th percentile value, they're in the top tier
    /// - Lower percentile values mean better performance (faster times)
    /// </summary>
    private static decimal CalculatePercentileLowerIsBetter(
        decimal athleteValue,
        (decimal percentile, decimal value)[] brackets)
    {
        // Check if athlete is better than the best bracket (95th percentile = fastest)
        if (athleteValue <= brackets[4].value)
        {
            return 95m + (5m * (brackets[4].value - athleteValue) /
                Math.Max(1m, brackets[4].value - brackets[3].value));
        }

        // Check if athlete is worse than the lowest bracket (20th percentile = slowest)
        if (athleteValue >= brackets[0].value)
        {
            return Math.Max(0m, 20m * brackets[0].value / Math.Max(1m, athleteValue));
        }

        // Find which bracket the athlete falls into and interpolate
        // For lower-is-better: higher bracket index = better performance (faster)
        for (int i = brackets.Length - 1; i > 0; i--)
        {
            var higherBracket = brackets[i];     // Better performance (lower value, higher percentile)
            var lowerBracket = brackets[i - 1];  // Worse performance (higher value, lower percentile)

            if (athleteValue >= higherBracket.value && athleteValue <= lowerBracket.value)
            {
                // Interpolate between brackets
                var range = lowerBracket.value - higherBracket.value;
                if (range <= 0) return higherBracket.percentile;

                var position = lowerBracket.value - athleteValue;
                var percentileRange = higherBracket.percentile - lowerBracket.percentile;

                return lowerBracket.percentile + (percentileRange * position / range);
            }
        }

        // Fallback (shouldn't reach here)
        return 50m;
    }

    #endregion
}
