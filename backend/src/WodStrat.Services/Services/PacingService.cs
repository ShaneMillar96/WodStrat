using Microsoft.EntityFrameworkCore;
using WodStrat.Dal.Enums;
using WodStrat.Dal.Interfaces;
using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;
using WodStrat.Services.Extensions;
using WodStrat.Services.Interfaces;
using WodStrat.Services.Utilities;

namespace WodStrat.Services.Services;

/// <summary>
/// Service implementation for calculating personalized pacing recommendations
/// based on athlete benchmark performance relative to population averages.
/// </summary>
public class PacingService : IPacingService
{
    // Percentile thresholds for pacing levels
    // Note: The actual thresholds are based on the plan (80th for Heavy, 60th for Moderate)
    // but the existing PacingLevel enum uses 60th for Heavy, 40th for Moderate
    // We'll use the plan's thresholds as they're more aligned with the feature requirements
    private const decimal HeavyPercentileThreshold = 80m;
    private const decimal ModeratePercentileThreshold = 60m;

    // Default rep count when not specified
    private const int DefaultRepCount = 10;

    private readonly IWodStratDatabase _database;
    private readonly IAthleteService _athleteService;

    /// <summary>
    /// Initializes a new instance of the PacingService.
    /// </summary>
    /// <param name="database">Database access interface.</param>
    /// <param name="athleteService">Athlete service for current user operations.</param>
    public PacingService(
        IWodStratDatabase database,
        IAthleteService athleteService)
    {
        _database = database;
        _athleteService = athleteService;
    }

    /// <inheritdoc />
    public async Task<WorkoutPacingResultDto?> CalculateWorkoutPacingAsync(
        int athleteId,
        int workoutId,
        CancellationToken cancellationToken = default)
    {
        // Load the workout with movements
        var workout = await _database.Get<Workout>()
            .Include(w => w.Movements)
                .ThenInclude(m => m.MovementDefinition)
            .Where(w => w.Id == workoutId && !w.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (workout == null)
        {
            return null;
        }

        // Calculate pacing for each movement
        var movementPacingList = new List<MovementPacingDto>();

        foreach (var workoutMovement in workout.Movements.OrderBy(m => m.SequenceOrder))
        {
            var repCount = workoutMovement.RepCount ?? DefaultRepCount;
            var movementPacing = await CalculateMovementPacingInternalAsync(
                athleteId,
                workoutMovement.MovementDefinition,
                repCount,
                cancellationToken);

            movementPacingList.Add(movementPacing);
        }

        // Generate overall strategy notes
        var strategyNotes = WorkoutStrategyGenerator.GenerateStrategyNotes(
            workout.WorkoutType,
            movementPacingList);

        return workout.ToWorkoutPacingResultDto(movementPacingList, strategyNotes);
    }

    /// <inheritdoc />
    public async Task<MovementPacingDto?> CalculateMovementPacingAsync(
        int athleteId,
        int movementDefinitionId,
        int repCount,
        CancellationToken cancellationToken = default)
    {
        // Load the movement definition
        var movementDefinition = await _database.Get<MovementDefinition>()
            .Where(m => m.Id == movementDefinitionId && !m.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (movementDefinition == null)
        {
            return null;
        }

        return await CalculateMovementPacingInternalAsync(
            athleteId,
            movementDefinition,
            repCount,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PacingLevel> DetermineAthletePacingLevelAsync(
        int athleteId,
        int benchmarkDefinitionId,
        CancellationToken cancellationToken = default)
    {
        // Get the benchmark definition to know the metric type
        var benchmarkDefinition = await _database.Get<BenchmarkDefinition>()
            .Where(b => b.Id == benchmarkDefinitionId && b.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (benchmarkDefinition == null)
        {
            return PacingLevel.Moderate; // Default fallback
        }

        // Get the athlete's benchmark value
        var athleteBenchmark = await _database.Get<AthleteBenchmark>()
            .Where(ab => ab.AthleteId == athleteId
                && ab.BenchmarkDefinitionId == benchmarkDefinitionId
                && !ab.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (athleteBenchmark == null)
        {
            return PacingLevel.Moderate; // No data - default to moderate
        }

        // Get population percentile data
        var populationData = await _database.Get<PopulationBenchmarkPercentile>()
            .Where(p => p.BenchmarkDefinitionId == benchmarkDefinitionId)
            .FirstOrDefaultAsync(cancellationToken);

        if (populationData == null)
        {
            return PacingLevel.Moderate; // No population data - default to moderate
        }

        // Calculate the athlete's percentile
        var percentile = CalculateAthletePercentile(
            athleteBenchmark.Value,
            populationData,
            benchmarkDefinition.MetricType);

        // Determine pacing level based on percentile
        return DeterminePacingLevelFromPercentile(percentile);
    }

    /// <inheritdoc />
    public decimal CalculateAthletePercentile(
        decimal athleteValue,
        PopulationBenchmarkPercentile populationData,
        BenchmarkMetricType metricType)
    {
        // Define the percentile brackets from the population data
        // The model has: Percentile20, Percentile40, Percentile60, Percentile80, Percentile95
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
            // For lower-is-better metrics:
            // If athlete value is LOWER than 95th percentile value, they're in the top tier
            // Lower percentile values mean better performance (faster times)

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
        }
        else
        {
            // For higher-is-better metrics:
            // If athlete value is HIGHER than 95th percentile value, they're in the top tier

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
                var lowerBracket = brackets[i];      // Lower percentile
                var higherBracket = brackets[i + 1]; // Higher percentile

                if (athleteValue >= lowerBracket.value && athleteValue <= higherBracket.value)
                {
                    // Interpolate between brackets
                    var range = higherBracket.value - lowerBracket.value;
                    if (range <= 0) return lowerBracket.percentile;

                    var position = athleteValue - lowerBracket.value;
                    var percentileRange = higherBracket.percentile - lowerBracket.percentile;

                    return lowerBracket.percentile + (percentileRange * position / range);
                }
            }
        }

        // Fallback (shouldn't reach here)
        return 50m;
    }

    /// <inheritdoc />
    public int[] CalculateSetBreakdown(int totalReps, PacingLevel pacingLevel)
    {
        if (totalReps <= 0)
        {
            return Array.Empty<int>();
        }

        return pacingLevel switch
        {
            PacingLevel.Heavy => CalculateHeavySetBreakdown(totalReps),
            PacingLevel.Moderate => CalculateModerateSetBreakdown(totalReps),
            PacingLevel.Light => CalculateLightSetBreakdown(totalReps),
            _ => CalculateModerateSetBreakdown(totalReps)
        };
    }

    /// <inheritdoc />
    public string GenerateGuidanceText(
        int totalReps,
        PacingLevel pacingLevel,
        int[] setBreakdown,
        string movementName)
    {
        if (totalReps <= 0 || setBreakdown.Length == 0)
        {
            return $"Complete {movementName} as prescribed.";
        }

        var breakdownText = string.Join("-", setBreakdown);

        return pacingLevel switch
        {
            PacingLevel.Heavy when setBreakdown.Length == 1 =>
                $"Go unbroken on {movementName} ({totalReps} reps). This is a strength - push the pace!",

            PacingLevel.Heavy =>
                $"Push hard on {movementName}. Aim for large sets ({breakdownText}) or go unbroken if possible.",

            PacingLevel.Moderate when setBreakdown.Length == 1 =>
                $"Controlled pace on {movementName} ({totalReps} reps). Stay steady.",

            PacingLevel.Moderate =>
                $"Controlled pace on {movementName}, break into sets of {breakdownText}. Maintain consistent effort.",

            PacingLevel.Light when setBreakdown.Length == 1 =>
                $"Conservative pace on {movementName} ({totalReps} reps). Don't burn out here.",

            PacingLevel.Light =>
                $"Conservative pace on {movementName}, break into manageable sets of {breakdownText}. Protect your energy for other movements.",

            _ => $"Complete {totalReps} {movementName} in sets of {breakdownText}."
        };
    }

    /// <inheritdoc />
    public async Task<WorkoutPacingResultDto?> CalculateCurrentUserWorkoutPacingAsync(
        int workoutId,
        CancellationToken cancellationToken = default)
    {
        var athlete = await _athleteService.GetCurrentUserAthleteAsync(cancellationToken);
        if (athlete == null)
        {
            return null;
        }

        return await CalculateWorkoutPacingAsync(athlete.Id, workoutId, cancellationToken);
    }

    #region Private Helper Methods

    /// <summary>
    /// Internal method to calculate movement pacing with a pre-loaded movement definition.
    /// </summary>
    private async Task<MovementPacingDto> CalculateMovementPacingInternalAsync(
        int athleteId,
        MovementDefinition movementDefinition,
        int repCount,
        CancellationToken cancellationToken)
    {
        // Find the benchmark mapping for this movement, prioritizing mappings where athlete has a benchmark
        var benchmarkMapping = await FindBestBenchmarkMappingAsync(athleteId, movementDefinition.Id, cancellationToken);

        // Initialize default values
        var pacingLevel = PacingLevel.Moderate;
        var percentile = 50m;
        var hasPopulationData = false;
        var hasAthleteBenchmark = false;
        var benchmarkName = "None";

        if (benchmarkMapping != null)
        {
            benchmarkName = benchmarkMapping.BenchmarkDefinition.Name;

            // Get athlete's benchmark for this definition
            var athleteBenchmark = await _database.Get<AthleteBenchmark>()
                .Where(ab => ab.AthleteId == athleteId
                    && ab.BenchmarkDefinitionId == benchmarkMapping.BenchmarkDefinitionId
                    && !ab.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            // Get population percentile data
            var populationData = await _database.Get<PopulationBenchmarkPercentile>()
                .Where(p => p.BenchmarkDefinitionId == benchmarkMapping.BenchmarkDefinitionId)
                .FirstOrDefaultAsync(cancellationToken);

            hasPopulationData = populationData != null;
            hasAthleteBenchmark = athleteBenchmark != null;

            if (hasPopulationData && hasAthleteBenchmark)
            {
                percentile = CalculateAthletePercentile(
                    athleteBenchmark!.Value,
                    populationData!,
                    benchmarkMapping.BenchmarkDefinition.MetricType);

                pacingLevel = DeterminePacingLevelFromPercentile(percentile);
            }
        }

        // Calculate set breakdown
        var setBreakdown = CalculateSetBreakdown(repCount, pacingLevel);

        // Generate guidance text
        var guidanceText = GenerateGuidanceText(repCount, pacingLevel, setBreakdown, movementDefinition.DisplayName);

        return movementDefinition.ToMovementPacingDto(
            pacingLevel,
            percentile,
            setBreakdown,
            guidanceText,
            benchmarkName,
            hasPopulationData,
            hasAthleteBenchmark);
    }

    /// <summary>
    /// Finds the best benchmark mapping for a movement, prioritizing mappings where the athlete has recorded a benchmark.
    /// This ensures we use the athlete's actual data when available instead of defaulting to arbitrary ordering.
    /// </summary>
    private async Task<BenchmarkMovementMapping?> FindBestBenchmarkMappingAsync(
        int athleteId,
        int movementDefinitionId,
        CancellationToken cancellationToken)
    {
        // Get all benchmark mappings for this movement
        var allMappings = await _database.Get<BenchmarkMovementMapping>()
            .Include(m => m.BenchmarkDefinition)
            .Where(m => m.MovementDefinitionId == movementDefinitionId)
            .OrderByDescending(m => m.RelevanceFactor)
            .ToListAsync(cancellationToken);

        if (allMappings.Count == 0)
        {
            return null;
        }

        // Get all benchmark definition IDs that the athlete has recorded
        var athleteBenchmarkDefIds = await _database.Get<AthleteBenchmark>()
            .Where(ab => ab.AthleteId == athleteId && !ab.IsDeleted)
            .Select(ab => ab.BenchmarkDefinitionId)
            .ToListAsync(cancellationToken);

        // First, try to find a mapping where the athlete has a benchmark (prioritize by relevance)
        var mappingWithAthleteBenchmark = allMappings
            .Where(m => athleteBenchmarkDefIds.Contains(m.BenchmarkDefinitionId))
            .OrderByDescending(m => m.RelevanceFactor)
            .FirstOrDefault();

        if (mappingWithAthleteBenchmark != null)
        {
            return mappingWithAthleteBenchmark;
        }

        // Fall back to the first mapping by relevance (original behavior)
        return allMappings[0];
    }

    /// <summary>
    /// Determines the pacing level based on calculated percentile.
    /// </summary>
    private static PacingLevel DeterminePacingLevelFromPercentile(decimal percentile)
    {
        if (percentile >= HeavyPercentileThreshold)
        {
            return PacingLevel.Heavy;
        }
        else if (percentile >= ModeratePercentileThreshold)
        {
            return PacingLevel.Moderate;
        }
        else
        {
            return PacingLevel.Light;
        }
    }

    /// <summary>
    /// Calculates set breakdown for Heavy pacing (athlete strength).
    /// Favors unbroken or large sets.
    /// </summary>
    private static int[] CalculateHeavySetBreakdown(int totalReps)
    {
        // If 15 or fewer reps, go unbroken
        if (totalReps <= 15)
        {
            return new[] { totalReps };
        }

        // If 25 or fewer reps, split into 2 sets favoring the first
        if (totalReps <= 25)
        {
            var firstSet = (int)Math.Ceiling(totalReps * 0.6);
            var secondSet = totalReps - firstSet;
            return new[] { firstSet, secondSet };
        }

        // For larger rep counts, use sets of ~15 max
        var sets = new List<int>();
        var remaining = totalReps;
        var maxSetSize = 15;

        while (remaining > 0)
        {
            var setSize = Math.Min(remaining, maxSetSize);
            sets.Add(setSize);
            remaining -= setSize;
        }

        return sets.ToArray();
    }

    /// <summary>
    /// Calculates set breakdown for Moderate pacing.
    /// Targets 2-3 evenly distributed sets with max ~10-12 reps per set.
    /// </summary>
    private static int[] CalculateModerateSetBreakdown(int totalReps)
    {
        // Small rep counts can be done in 1-2 sets
        if (totalReps <= 6)
        {
            return new[] { totalReps };
        }

        if (totalReps <= 12)
        {
            var firstSet = (int)Math.Ceiling(totalReps / 2.0);
            var secondSet = totalReps - firstSet;
            return new[] { firstSet, secondSet };
        }

        // Target 10-12 reps per set
        var targetSetSize = 10;
        var numberOfSets = (int)Math.Ceiling((double)totalReps / targetSetSize);

        // Distribute reps evenly
        var sets = new List<int>();
        var remaining = totalReps;

        for (int i = 0; i < numberOfSets; i++)
        {
            var repsPerRemainingSet = (int)Math.Ceiling((double)remaining / (numberOfSets - i));
            sets.Add(repsPerRemainingSet);
            remaining -= repsPerRemainingSet;
        }

        return sets.ToArray();
    }

    /// <summary>
    /// Calculates set breakdown for Light pacing (athlete weakness).
    /// Targets smaller, manageable sets with max ~7 reps per set.
    /// </summary>
    private static int[] CalculateLightSetBreakdown(int totalReps)
    {
        // Even small rep counts should be broken up
        if (totalReps <= 3)
        {
            return new[] { totalReps };
        }

        // Target 7 reps per set max, favor equal distribution
        var targetSetSize = 7;
        var numberOfSets = (int)Math.Ceiling((double)totalReps / targetSetSize);

        // For classic rep schemes like 21, prefer equal splits (7-7-7)
        if (totalReps % numberOfSets == 0)
        {
            var setSize = totalReps / numberOfSets;
            return Enumerable.Repeat(setSize, numberOfSets).ToArray();
        }

        // Distribute reps evenly
        var sets = new List<int>();
        var remaining = totalReps;

        for (int i = 0; i < numberOfSets; i++)
        {
            var repsPerRemainingSet = (int)Math.Ceiling((double)remaining / (numberOfSets - i));
            sets.Add(repsPerRemainingSet);
            remaining -= repsPerRemainingSet;
        }

        return sets.ToArray();
    }

    #endregion
}
