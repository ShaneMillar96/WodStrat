using Microsoft.EntityFrameworkCore;
using WodStrat.Dal.Enums;
using WodStrat.Dal.Interfaces;
using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;
using WodStrat.Services.Extensions;
using WodStrat.Services.Interfaces;

namespace WodStrat.Services.Services;

/// <summary>
/// Service implementation for calculating personalized time estimates and feasibility
/// assessments for workouts based on athlete benchmark data and performance characteristics.
/// </summary>
public class TimeEstimateService : ITimeEstimateService
{
    // Base time per rep by movement category (in seconds)
    private const decimal GymnasticsSecondsPerRep = 2.5m;
    private const decimal WeightliftingSecondsPerRep = 4.0m;
    private const decimal StrongmanSecondsPerRep = 5.0m;
    private const decimal AccessorySecondsPerRep = 2.0m;

    // Cardio base times
    private const decimal RowSecondsPerMeter = 0.2m;    // ~500m in 100s (2:00/500m pace)
    private const decimal RunSecondsPerMeter = 0.24m;   // ~400m in 96s
    private const decimal BikeSecondsPerCalorie = 2.5m; // ~12 cal/min

    // EMOM buffer thresholds
    private const int MinimumBufferSeconds = 5;
    private const int ComfortableBufferSeconds = 10;

    // Rest recommendation by pacing level (seconds)
    private static readonly (int min, int max) LightRest = (15, 20);
    private static readonly (int min, int max) ModerateRest = (8, 12);
    private static readonly (int min, int max) HeavyRest = (3, 5);

    private readonly IWodStratDatabase _database;
    private readonly IAthleteService _athleteService;
    private readonly IPacingService _pacingService;

    /// <summary>
    /// Initializes a new instance of the TimeEstimateService.
    /// </summary>
    public TimeEstimateService(
        IWodStratDatabase database,
        IAthleteService athleteService,
        IPacingService pacingService)
    {
        _database = database;
        _athleteService = athleteService;
        _pacingService = pacingService;
    }

    /// <inheritdoc />
    public async Task<TimeEstimateResultDto?> EstimateWorkoutTimeAsync(
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

        // Route to appropriate workout-type-specific method
        return workout.WorkoutType switch
        {
            WorkoutType.ForTime => await EstimateForTimeWorkoutAsync(athleteId, workout, cancellationToken),
            WorkoutType.Amrap => await EstimateAmrapWorkoutAsync(athleteId, workout, cancellationToken),
            WorkoutType.Emom => await EstimateEmomWorkoutAsync(athleteId, workout, cancellationToken),
            WorkoutType.Intervals => await EstimateIntervalWorkoutAsync(athleteId, workout, cancellationToken),
            WorkoutType.Rounds => await EstimateForTimeWorkoutAsync(athleteId, workout, cancellationToken), // Treat Rounds like ForTime
            WorkoutType.Tabata => await EstimateTabataWorkoutAsync(athleteId, workout, cancellationToken),
            _ => await EstimateForTimeWorkoutAsync(athleteId, workout, cancellationToken)
        };
    }

    /// <inheritdoc />
    public async Task<TimeEstimateResultDto> EstimateForTimeWorkoutAsync(
        int athleteId,
        Workout workout,
        CancellationToken cancellationToken = default)
    {
        var athlete = await GetAthleteAsync(athleteId, cancellationToken);
        var movements = workout.Movements.OrderBy(m => m.SequenceOrder).ToList();

        // Calculate base time for all movements
        var (baseTimeSeconds, movementPacingLevels, percentiles) = await CalculateBaseWorkoutTimeAsync(
            athleteId, movements, cancellationToken);

        // Account for rounds if specified
        var totalRounds = workout.RoundCount ?? 1;
        baseTimeSeconds *= totalRounds;

        // Calculate benchmark coverage
        var benchmarkCoverage = movements.Count > 0
            ? (movementPacingLevels.Count * 100) / movements.Count
            : 0;

        var averagePercentile = percentiles.Count > 0 ? percentiles.Average() : 50m;

        // Calculate time range based on athlete performance
        var (minSeconds, maxSeconds) = CalculateTimeRange(
            baseTimeSeconds,
            averagePercentile,
            athlete?.ExperienceLevel ?? ExperienceLevel.Intermediate,
            benchmarkCoverage);

        // Generate rest recommendations
        var overallPacing = DetermineOverallPacing(percentiles);
        var restRecommendations = CalculateRestRecommendations(movements, overallPacing, movementPacingLevels);

        // Generate factors summary
        var confidenceLevel = TimeEstimateConfidenceLevel.FromCoverage(benchmarkCoverage);
        var factorsSummary = GenerateFactorsSummary(
            athlete?.ExperienceLevel ?? ExperienceLevel.Intermediate,
            benchmarkCoverage,
            movements.Count,
            averagePercentile);

        return workout.ToTimeEstimateResultDto(
            minSeconds,
            maxSeconds,
            confidenceLevel,
            factorsSummary,
            restRecommendations,
            movementPacingLevels.Count,
            movements.Count,
            averagePercentile);
    }

    /// <inheritdoc />
    public async Task<TimeEstimateResultDto> EstimateAmrapWorkoutAsync(
        int athleteId,
        Workout workout,
        CancellationToken cancellationToken = default)
    {
        var athlete = await GetAthleteAsync(athleteId, cancellationToken);
        var movements = workout.Movements.OrderBy(m => m.SequenceOrder).ToList();

        // Calculate time for one round
        var (roundTimeSeconds, movementPacingLevels, percentiles) = await CalculateBaseWorkoutTimeAsync(
            athleteId, movements, cancellationToken);

        if (roundTimeSeconds <= 0)
        {
            roundTimeSeconds = 60; // Default to 1 minute per round if no data
        }

        // Calculate reps per round
        var repsPerRound = movements.Sum(m => m.RepCount ?? 10);

        // Get time cap
        var timeCapSeconds = workout.TimeCapSeconds ?? 720; // Default to 12 minutes

        // Calculate benchmark coverage
        var benchmarkCoverage = movements.Count > 0
            ? (movementPacingLevels.Count * 100) / movements.Count
            : 0;

        var averagePercentile = percentiles.Count > 0 ? percentiles.Average() : 50m;

        // Adjust round time based on athlete percentile
        var adjustmentFactor = GetPercentileAdjustmentFactor(averagePercentile);
        var adjustedRoundTime = (int)(roundTimeSeconds * adjustmentFactor);

        // Calculate range width based on experience and coverage
        var rangeWidth = GetRangeWidth(
            athlete?.ExperienceLevel ?? ExperienceLevel.Intermediate,
            benchmarkCoverage);

        // Calculate rounds range
        var avgRoundTime = Math.Max(adjustedRoundTime, 1);
        var estimatedRounds = (decimal)timeCapSeconds / avgRoundTime;

        var minRoundsDecimal = estimatedRounds * (1 - rangeWidth);
        var maxRoundsDecimal = estimatedRounds * (1 + rangeWidth);

        var minRounds = (int)Math.Floor(minRoundsDecimal);
        var maxRounds = (int)Math.Floor(maxRoundsDecimal);

        var minExtraReps = (int)((minRoundsDecimal - minRounds) * repsPerRound);
        var maxExtraReps = (int)((maxRoundsDecimal - maxRounds) * repsPerRound);

        // Ensure min is less than max
        if (minRounds > maxRounds || (minRounds == maxRounds && minExtraReps > maxExtraReps))
        {
            (minRounds, maxRounds) = (maxRounds, minRounds);
            (minExtraReps, maxExtraReps) = (maxExtraReps, minExtraReps);
        }

        // Generate rest recommendations
        var overallPacing = DetermineOverallPacing(percentiles);
        var restRecommendations = CalculateRestRecommendations(movements, overallPacing, movementPacingLevels);

        // Generate factors summary
        var confidenceLevel = TimeEstimateConfidenceLevel.FromCoverage(benchmarkCoverage);
        var factorsSummary = GenerateFactorsSummary(
            athlete?.ExperienceLevel ?? ExperienceLevel.Intermediate,
            benchmarkCoverage,
            movements.Count,
            averagePercentile);

        return workout.ToAmrapEstimateResultDto(
            minRounds,
            minExtraReps,
            maxRounds,
            maxExtraReps,
            repsPerRound,
            confidenceLevel,
            factorsSummary,
            restRecommendations,
            movementPacingLevels.Count,
            movements.Count,
            averagePercentile);
    }

    /// <inheritdoc />
    public async Task<List<EmomFeasibilityDto>> CheckEmomFeasibilityAsync(
        int athleteId,
        Workout workout,
        CancellationToken cancellationToken = default)
    {
        var movements = workout.Movements.OrderBy(m => m.SequenceOrder).ToList();
        var feasibilityList = new List<EmomFeasibilityDto>();

        // Group movements by minute
        var totalMinutes = workout.TimeCapSeconds.HasValue
            ? workout.TimeCapSeconds.Value / 60
            : movements.Max(m => m.MinuteEnd ?? m.MinuteStart ?? 1);

        var intervalDuration = workout.IntervalDurationSeconds ?? 60;

        for (var minute = 1; minute <= totalMinutes; minute++)
        {
            // Get movements for this minute
            var minuteMovements = movements
                .Where(m => (m.MinuteStart ?? 1) <= minute && (m.MinuteEnd ?? m.MinuteStart ?? 1) >= minute)
                .ToList();

            if (minuteMovements.Count == 0)
            {
                // Check for alternating EMOM pattern
                var cycleMinute = (minute - 1) % Math.Max(movements.Count, 1);
                if (movements.Count > 0)
                {
                    minuteMovements = new List<WorkoutMovement> { movements.ElementAtOrDefault(cycleMinute) ?? movements.First() };
                }
            }

            // Calculate estimated time for this minute's work
            var (minuteTime, _, _) = await CalculateBaseWorkoutTimeAsync(
                athleteId, minuteMovements, cancellationToken);

            var bufferSeconds = intervalDuration - minuteTime;
            var isFeasible = bufferSeconds >= MinimumBufferSeconds;

            var recommendation = bufferSeconds switch
            {
                >= ComfortableBufferSeconds => "On pace - comfortable buffer",
                >= MinimumBufferSeconds => "Tight timing - maintain focus",
                _ => "Consider scaling - insufficient recovery time"
            };

            var prescribedWork = string.Join(" + ", minuteMovements.Select(m =>
                $"{m.RepCount ?? 10} {m.MovementDefinition.DisplayName}"));

            feasibilityList.Add(new EmomFeasibilityDto
            {
                Minute = minute,
                PrescribedWork = prescribedWork,
                EstimatedCompletionSeconds = minuteTime,
                IsFeasible = isFeasible,
                BufferSeconds = Math.Max(bufferSeconds, 0),
                Recommendation = recommendation,
                MovementNames = minuteMovements.Select(m => m.MovementDefinition.DisplayName).ToList()
            });
        }

        return feasibilityList;
    }

    /// <inheritdoc />
    public async Task<TimeEstimateResultDto> EstimateIntervalWorkoutAsync(
        int athleteId,
        Workout workout,
        CancellationToken cancellationToken = default)
    {
        var athlete = await GetAthleteAsync(athleteId, cancellationToken);
        var movements = workout.Movements.OrderBy(m => m.SequenceOrder).ToList();

        // Calculate work time
        var (workTimeSeconds, movementPacingLevels, percentiles) = await CalculateBaseWorkoutTimeAsync(
            athleteId, movements, cancellationToken);

        // For interval workouts, total time is based on prescribed intervals
        var intervalDuration = workout.IntervalDurationSeconds ?? 60;
        var rounds = workout.RoundCount ?? 1;
        var totalSeconds = intervalDuration * rounds;

        // Calculate benchmark coverage
        var benchmarkCoverage = movements.Count > 0
            ? (movementPacingLevels.Count * 100) / movements.Count
            : 0;

        var averagePercentile = percentiles.Count > 0 ? percentiles.Average() : 50m;

        // Generate rest recommendations
        var overallPacing = DetermineOverallPacing(percentiles);
        var restRecommendations = CalculateRestRecommendations(movements, overallPacing, movementPacingLevels);

        // Generate factors summary
        var confidenceLevel = TimeEstimateConfidenceLevel.FromCoverage(benchmarkCoverage);
        var factorsSummary = GenerateFactorsSummary(
            athlete?.ExperienceLevel ?? ExperienceLevel.Intermediate,
            benchmarkCoverage,
            movements.Count,
            averagePercentile);

        return workout.ToIntervalsEstimateResultDto(
            totalSeconds,
            confidenceLevel,
            factorsSummary,
            restRecommendations,
            movementPacingLevels.Count,
            movements.Count,
            averagePercentile);
    }

    /// <inheritdoc />
    public (int minSeconds, int maxSeconds) CalculateTimeRange(
        int baseTimeSeconds,
        decimal athletePercentile,
        ExperienceLevel experience,
        int benchmarkCoveragePercent)
    {
        // Apply adjustment factor based on percentile
        var adjustmentFactor = GetPercentileAdjustmentFactor(athletePercentile);
        var adjustedTime = (int)(baseTimeSeconds * adjustmentFactor);

        // Calculate range width
        var rangeWidth = GetRangeWidth(experience, benchmarkCoveragePercent);

        var minSeconds = (int)(adjustedTime * (1 - rangeWidth));
        var maxSeconds = (int)(adjustedTime * (1 + rangeWidth));

        // Ensure minimum reasonable values
        minSeconds = Math.Max(minSeconds, 1);
        maxSeconds = Math.Max(maxSeconds, minSeconds + 1);

        return (minSeconds, maxSeconds);
    }

    /// <inheritdoc />
    public List<RestRecommendationDto> CalculateRestRecommendations(
        List<WorkoutMovement> movements,
        PacingLevel overallPacing,
        Dictionary<int, PacingLevel> movementPacingLevels)
    {
        var recommendations = new List<RestRecommendationDto>();

        foreach (var movement in movements)
        {
            var pacingLevel = movementPacingLevels.TryGetValue(movement.MovementDefinitionId, out var level)
                ? level
                : overallPacing;

            var (minRest, maxRest) = pacingLevel switch
            {
                PacingLevel.Light => LightRest,
                PacingLevel.Heavy => HeavyRest,
                _ => ModerateRest
            };

            var suggestedRest = (minRest + maxRest) / 2;
            var restRange = $"{minRest}-{maxRest} seconds";

            var reasoning = pacingLevel switch
            {
                PacingLevel.Light => "This is a weakness - longer recovery needed to maintain quality",
                PacingLevel.Heavy => "This is a strength - quick transitions to capitalize on advantage",
                _ => "Average performance - maintain steady output with moderate rest"
            };

            recommendations.Add(movement.MovementDefinition.ToRestRecommendationDto(
                pacingLevel,
                suggestedRest,
                restRange,
                reasoning));
        }

        return recommendations;
    }

    /// <inheritdoc />
    public string FormatTimeRange(int minSeconds, int maxSeconds)
    {
        return TimeEstimateMappingExtensions.FormatTimeRange(minSeconds, maxSeconds);
    }

    /// <inheritdoc />
    public string FormatAmrapRange(int minRounds, int minReps, int maxRounds, int maxReps, int repsPerRound)
    {
        return TimeEstimateMappingExtensions.FormatAmrapRange(minRounds, minReps, maxRounds, maxReps, repsPerRound);
    }

    /// <inheritdoc />
    public async Task<TimeEstimateResultDto?> EstimateCurrentUserWorkoutTimeAsync(
        int workoutId,
        CancellationToken cancellationToken = default)
    {
        var athlete = await _athleteService.GetCurrentUserAthleteAsync(cancellationToken);
        if (athlete == null)
        {
            return null;
        }

        return await EstimateWorkoutTimeAsync(athlete.Id, workoutId, cancellationToken);
    }

    #region Private Helper Methods

    /// <summary>
    /// Estimates EMOM workout with feasibility data.
    /// </summary>
    private async Task<TimeEstimateResultDto> EstimateEmomWorkoutAsync(
        int athleteId,
        Workout workout,
        CancellationToken cancellationToken)
    {
        var athlete = await GetAthleteAsync(athleteId, cancellationToken);
        var movements = workout.Movements.OrderBy(m => m.SequenceOrder).ToList();

        // Get feasibility for all minutes
        var feasibilityList = await CheckEmomFeasibilityAsync(athleteId, workout, cancellationToken);

        // Calculate benchmark coverage
        var (_, movementPacingLevels, percentiles) = await CalculateBaseWorkoutTimeAsync(
            athleteId, movements, cancellationToken);

        var benchmarkCoverage = movements.Count > 0
            ? (movementPacingLevels.Count * 100) / movements.Count
            : 0;

        var averagePercentile = percentiles.Count > 0 ? percentiles.Average() : 50m;

        // Generate factors summary
        var confidenceLevel = TimeEstimateConfidenceLevel.FromCoverage(benchmarkCoverage);
        var factorsSummary = GenerateFactorsSummary(
            athlete?.ExperienceLevel ?? ExperienceLevel.Intermediate,
            benchmarkCoverage,
            movements.Count,
            averagePercentile);

        return workout.ToEmomEstimateResultDto(
            feasibilityList,
            confidenceLevel,
            factorsSummary,
            movementPacingLevels.Count,
            movements.Count,
            averagePercentile);
    }

    /// <summary>
    /// Estimates Tabata workout timing.
    /// </summary>
    private async Task<TimeEstimateResultDto> EstimateTabataWorkoutAsync(
        int athleteId,
        Workout workout,
        CancellationToken cancellationToken)
    {
        var athlete = await GetAthleteAsync(athleteId, cancellationToken);
        var movements = workout.Movements.OrderBy(m => m.SequenceOrder).ToList();

        // Tabata is 8 rounds of 20s work / 10s rest = 4 minutes per movement
        var tabataRounds = 8;
        var workSeconds = 20;
        var restSeconds = 10;
        var totalSecondsPerMovement = tabataRounds * (workSeconds + restSeconds);
        var totalSeconds = totalSecondsPerMovement * Math.Max(movements.Count, 1);

        // Calculate benchmark coverage
        var (_, movementPacingLevels, percentiles) = await CalculateBaseWorkoutTimeAsync(
            athleteId, movements, cancellationToken);

        var benchmarkCoverage = movements.Count > 0
            ? (movementPacingLevels.Count * 100) / movements.Count
            : 0;

        var averagePercentile = percentiles.Count > 0 ? percentiles.Average() : 50m;

        // Generate factors summary
        var confidenceLevel = TimeEstimateConfidenceLevel.FromCoverage(benchmarkCoverage);
        var factorsSummary = GenerateFactorsSummary(
            athlete?.ExperienceLevel ?? ExperienceLevel.Intermediate,
            benchmarkCoverage,
            movements.Count,
            averagePercentile);

        var overallPacing = DetermineOverallPacing(percentiles);
        var restRecommendations = CalculateRestRecommendations(movements, overallPacing, movementPacingLevels);

        return workout.ToIntervalsEstimateResultDto(
            totalSeconds,
            confidenceLevel,
            factorsSummary,
            restRecommendations,
            movementPacingLevels.Count,
            movements.Count,
            averagePercentile);
    }

    /// <summary>
    /// Calculates base workout time and gathers pacing/percentile data.
    /// </summary>
    private async Task<(int baseTimeSeconds, Dictionary<int, PacingLevel> movementPacingLevels, List<decimal> percentiles)>
        CalculateBaseWorkoutTimeAsync(
            int athleteId,
            List<WorkoutMovement> movements,
            CancellationToken cancellationToken)
    {
        var totalTimeSeconds = 0;
        var movementPacingLevels = new Dictionary<int, PacingLevel>();
        var percentiles = new List<decimal>();

        foreach (var movement in movements)
        {
            // Calculate movement time based on category
            var movementTime = CalculateMovementTime(movement);
            totalTimeSeconds += movementTime;

            // Get pacing level from existing pacing service
            var pacingDto = await _pacingService.CalculateMovementPacingAsync(
                athleteId,
                movement.MovementDefinitionId,
                movement.RepCount ?? 10,
                cancellationToken);

            if (pacingDto != null)
            {
                var pacingLevel = ParsePacingLevel(pacingDto.PacingLevel);
                movementPacingLevels[movement.MovementDefinitionId] = pacingLevel;
                percentiles.Add(pacingDto.AthletePercentile);
            }
        }

        return (totalTimeSeconds, movementPacingLevels, percentiles);
    }

    /// <summary>
    /// Calculates estimated time for a single movement.
    /// </summary>
    private static int CalculateMovementTime(WorkoutMovement movement)
    {
        var definition = movement.MovementDefinition;
        var repCount = movement.RepCount ?? 10;

        // Handle cardio/distance-based movements
        if (definition.Category == MovementCategory.Cardio)
        {
            return CalculateCardioTime(movement);
        }

        // Calculate rep-based movement time
        var secondsPerRep = definition.Category switch
        {
            MovementCategory.Gymnastics => GymnasticsSecondsPerRep,
            MovementCategory.Weightlifting => WeightliftingSecondsPerRep,
            MovementCategory.Strongman => StrongmanSecondsPerRep,
            MovementCategory.Accessory => AccessorySecondsPerRep,
            _ => GymnasticsSecondsPerRep
        };

        return (int)(repCount * secondsPerRep);
    }

    /// <summary>
    /// Calculates time for cardio movements.
    /// </summary>
    private static int CalculateCardioTime(WorkoutMovement movement)
    {
        // Handle distance-based
        if (movement.DistanceValue.HasValue)
        {
            var meters = ConvertToMeters(movement.DistanceValue.Value, movement.DistanceUnit);
            var canonicalName = movement.MovementDefinition.CanonicalName.ToLowerInvariant();

            // Adjust pace based on movement type
            var pacePerMeter = canonicalName switch
            {
                var n when n.Contains("row") => RowSecondsPerMeter,
                var n when n.Contains("run") => RunSecondsPerMeter,
                var n when n.Contains("bike") => 0.15m, // Faster on bike
                var n when n.Contains("ski") => RowSecondsPerMeter,
                _ => RowSecondsPerMeter
            };

            return (int)(meters * pacePerMeter);
        }

        // Handle calorie-based
        if (movement.Calories.HasValue)
        {
            return (int)(movement.Calories.Value * BikeSecondsPerCalorie);
        }

        // Handle duration-based (holds, etc.)
        if (movement.DurationSeconds.HasValue)
        {
            return movement.DurationSeconds.Value;
        }

        // Default for cardio without specifics
        return 60;
    }

    /// <summary>
    /// Converts distance to meters.
    /// </summary>
    private static decimal ConvertToMeters(decimal value, DistanceUnit? unit)
    {
        return unit switch
        {
            DistanceUnit.Km => value * 1000m,
            DistanceUnit.Ft => value * 0.3048m,
            DistanceUnit.Mi => value * 1609.34m,
            DistanceUnit.Cal => value, // Not a real conversion, but calories handled separately
            DistanceUnit.M => value,
            _ => value // Assume meters
        };
    }

    /// <summary>
    /// Gets adjustment factor based on athlete percentile.
    /// </summary>
    private static decimal GetPercentileAdjustmentFactor(decimal percentile)
    {
        // Higher percentile = faster completion = lower factor
        return percentile switch
        {
            >= 95m => 0.85m,  // 15% faster
            >= 80m => 0.92m,  // 8% faster
            >= 60m => 1.00m,  // Baseline
            >= 40m => 1.10m,  // 10% slower
            >= 20m => 1.25m,  // 25% slower
            _ => 1.40m        // 40% slower
        };
    }

    /// <summary>
    /// Gets range width based on experience and benchmark coverage.
    /// </summary>
    private static decimal GetRangeWidth(ExperienceLevel experience, int benchmarkCoveragePercent)
    {
        // Base width from experience
        var baseWidth = experience switch
        {
            ExperienceLevel.Beginner => 0.20m,     // +/- 20%
            ExperienceLevel.Intermediate => 0.15m, // +/- 15%
            ExperienceLevel.Advanced => 0.10m,     // +/- 10%
            _ => 0.15m
        };

        // Add width based on low benchmark coverage
        var coverageAdjustment = benchmarkCoveragePercent switch
        {
            < 50 => 0.10m,  // Low coverage adds +/- 10%
            < 80 => 0.05m,  // Medium coverage adds +/- 5%
            _ => 0m         // High coverage - no adjustment
        };

        return baseWidth + coverageAdjustment;
    }

    /// <summary>
    /// Determines overall pacing level from a list of percentiles.
    /// </summary>
    private static PacingLevel DetermineOverallPacing(List<decimal> percentiles)
    {
        if (percentiles.Count == 0)
        {
            return PacingLevel.Moderate;
        }

        var average = percentiles.Average();

        return average switch
        {
            >= 80m => PacingLevel.Heavy,
            >= 60m => PacingLevel.Moderate,
            _ => PacingLevel.Light
        };
    }

    /// <summary>
    /// Parses pacing level string to enum.
    /// </summary>
    private static PacingLevel ParsePacingLevel(string pacingLevel)
    {
        return pacingLevel.ToLowerInvariant() switch
        {
            "heavy" => PacingLevel.Heavy,
            "light" => PacingLevel.Light,
            _ => PacingLevel.Moderate
        };
    }

    /// <summary>
    /// Generates human-readable factors summary.
    /// </summary>
    private static string GenerateFactorsSummary(
        ExperienceLevel experience,
        int benchmarkCoverage,
        int totalMovements,
        decimal averagePercentile)
    {
        var factors = new List<string>();

        // Experience factor
        var experienceNote = experience switch
        {
            ExperienceLevel.Beginner => "Beginner experience (+/- 20% range)",
            ExperienceLevel.Intermediate => "Intermediate experience (+/- 15% range)",
            ExperienceLevel.Advanced => "Advanced experience (+/- 10% range)",
            _ => "Intermediate experience assumed"
        };
        factors.Add(experienceNote);

        // Coverage factor
        var coverageNote = benchmarkCoverage switch
        {
            >= 80 => $"High benchmark coverage ({benchmarkCoverage}%)",
            >= 50 => $"Medium benchmark coverage ({benchmarkCoverage}%)",
            _ => $"Low benchmark coverage ({benchmarkCoverage}%) - estimate less reliable"
        };
        factors.Add(coverageNote);

        // Performance factor
        var performanceNote = averagePercentile switch
        {
            >= 80m => $"Strong performer (avg {averagePercentile:F0}th percentile)",
            >= 60m => $"Above average performer (avg {averagePercentile:F0}th percentile)",
            >= 40m => $"Average performer (avg {averagePercentile:F0}th percentile)",
            _ => $"Developing athlete (avg {averagePercentile:F0}th percentile)"
        };
        factors.Add(performanceNote);

        return string.Join(". ", factors) + ".";
    }

    /// <summary>
    /// Gets athlete entity by ID.
    /// </summary>
    private async Task<Athlete?> GetAthleteAsync(int athleteId, CancellationToken cancellationToken)
    {
        return await _database.Get<Athlete>()
            .Where(a => a.Id == athleteId && !a.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    #endregion
}
