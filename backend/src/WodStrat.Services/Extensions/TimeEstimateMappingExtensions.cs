using WodStrat.Dal.Enums;
using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;

namespace WodStrat.Services.Extensions;

/// <summary>
/// Extension methods for mapping time estimate data.
/// </summary>
public static class TimeEstimateMappingExtensions
{
    /// <summary>
    /// Creates a TimeEstimateResultDto for a ForTime workout.
    /// </summary>
    public static TimeEstimateResultDto ToTimeEstimateResultDto(
        this Workout workout,
        int minSeconds,
        int maxSeconds,
        string confidenceLevel,
        string factorsSummary,
        IReadOnlyList<RestRecommendationDto> restRecommendations,
        int benchmarkCoverage,
        int totalMovements,
        decimal averagePercentile)
    {
        return new TimeEstimateResultDto
        {
            WorkoutId = workout.Id,
            WorkoutName = workout.Name ?? "Unnamed Workout",
            WorkoutType = workout.WorkoutType.ToString(),
            EstimateType = "Time",
            MinEstimate = minSeconds,
            MaxEstimate = maxSeconds,
            FormattedRange = FormatTimeRange(minSeconds, maxSeconds),
            ConfidenceLevel = confidenceLevel,
            FactorsSummary = factorsSummary,
            RestRecommendations = restRecommendations,
            CalculatedAt = DateTime.UtcNow,
            BenchmarkCoverageCount = benchmarkCoverage,
            TotalMovementCount = totalMovements,
            AveragePercentile = averagePercentile
        };
    }

    /// <summary>
    /// Creates a TimeEstimateResultDto for an AMRAP workout.
    /// </summary>
    public static TimeEstimateResultDto ToAmrapEstimateResultDto(
        this Workout workout,
        int minRounds,
        int minReps,
        int maxRounds,
        int maxReps,
        int repsPerRound,
        string confidenceLevel,
        string factorsSummary,
        IReadOnlyList<RestRecommendationDto> restRecommendations,
        int benchmarkCoverage,
        int totalMovements,
        decimal averagePercentile)
    {
        return new TimeEstimateResultDto
        {
            WorkoutId = workout.Id,
            WorkoutName = workout.Name ?? "Unnamed Workout",
            WorkoutType = workout.WorkoutType.ToString(),
            EstimateType = "RoundsReps",
            MinEstimate = minRounds,
            MaxEstimate = maxRounds,
            MinExtraReps = minReps,
            MaxExtraReps = maxReps,
            FormattedRange = FormatAmrapRange(minRounds, minReps, maxRounds, maxReps, repsPerRound),
            ConfidenceLevel = confidenceLevel,
            FactorsSummary = factorsSummary,
            RestRecommendations = restRecommendations,
            CalculatedAt = DateTime.UtcNow,
            BenchmarkCoverageCount = benchmarkCoverage,
            TotalMovementCount = totalMovements,
            AveragePercentile = averagePercentile
        };
    }

    /// <summary>
    /// Creates a TimeEstimateResultDto for an EMOM workout with feasibility data.
    /// </summary>
    public static TimeEstimateResultDto ToEmomEstimateResultDto(
        this Workout workout,
        IReadOnlyList<EmomFeasibilityDto> feasibilityList,
        string confidenceLevel,
        string factorsSummary,
        int benchmarkCoverage,
        int totalMovements,
        decimal averagePercentile)
    {
        var feasibleCount = feasibilityList.Count(f => f.IsFeasible);
        var totalMinutes = feasibilityList.Count;

        return new TimeEstimateResultDto
        {
            WorkoutId = workout.Id,
            WorkoutName = workout.Name ?? "Unnamed Workout",
            WorkoutType = workout.WorkoutType.ToString(),
            EstimateType = "Feasibility",
            MinEstimate = feasibleCount,
            MaxEstimate = totalMinutes,
            FormattedRange = $"{feasibleCount}/{totalMinutes} minutes feasible",
            ConfidenceLevel = confidenceLevel,
            FactorsSummary = factorsSummary,
            RestRecommendations = Array.Empty<RestRecommendationDto>(),
            EmomFeasibility = feasibilityList,
            CalculatedAt = DateTime.UtcNow,
            BenchmarkCoverageCount = benchmarkCoverage,
            TotalMovementCount = totalMovements,
            AveragePercentile = averagePercentile
        };
    }

    /// <summary>
    /// Creates a TimeEstimateResultDto for an Intervals workout.
    /// </summary>
    public static TimeEstimateResultDto ToIntervalsEstimateResultDto(
        this Workout workout,
        int totalEstimatedSeconds,
        string confidenceLevel,
        string factorsSummary,
        IReadOnlyList<RestRecommendationDto> restRecommendations,
        int benchmarkCoverage,
        int totalMovements,
        decimal averagePercentile)
    {
        return new TimeEstimateResultDto
        {
            WorkoutId = workout.Id,
            WorkoutName = workout.Name ?? "Unnamed Workout",
            WorkoutType = workout.WorkoutType.ToString(),
            EstimateType = "Duration",
            MinEstimate = totalEstimatedSeconds,
            MaxEstimate = totalEstimatedSeconds,
            FormattedRange = FormatDuration(totalEstimatedSeconds),
            ConfidenceLevel = confidenceLevel,
            FactorsSummary = factorsSummary,
            RestRecommendations = restRecommendations,
            CalculatedAt = DateTime.UtcNow,
            BenchmarkCoverageCount = benchmarkCoverage,
            TotalMovementCount = totalMovements,
            AveragePercentile = averagePercentile
        };
    }

    /// <summary>
    /// Creates a RestRecommendationDto for a movement.
    /// </summary>
    public static RestRecommendationDto ToRestRecommendationDto(
        this MovementDefinition movement,
        PacingLevel pacingLevel,
        int suggestedRestSeconds,
        string restRange,
        string reasoning)
    {
        return new RestRecommendationDto
        {
            AfterMovement = movement.DisplayName,
            MovementDefinitionId = movement.Id,
            SuggestedRestSeconds = suggestedRestSeconds,
            RestRange = restRange,
            Reasoning = reasoning,
            PacingLevel = pacingLevel.ToString()
        };
    }

    /// <summary>
    /// Formats a time range as MM:SS - MM:SS.
    /// </summary>
    public static string FormatTimeRange(int minSeconds, int maxSeconds)
    {
        var minTime = TimeSpan.FromSeconds(minSeconds);
        var maxTime = TimeSpan.FromSeconds(maxSeconds);

        return $"{FormatTime(minTime)} - {FormatTime(maxTime)}";
    }

    /// <summary>
    /// Formats an AMRAP range as "X+Y to Z+W rounds".
    /// </summary>
    public static string FormatAmrapRange(
        int minRounds,
        int minReps,
        int maxRounds,
        int maxReps,
        int repsPerRound)
    {
        var minDisplay = minReps > 0 ? $"{minRounds}+{minReps}" : $"{minRounds}";
        var maxDisplay = maxReps > 0 ? $"{maxRounds}+{maxReps}" : $"{maxRounds}";

        return $"{minDisplay} to {maxDisplay} rounds";
    }

    /// <summary>
    /// Formats a duration as a human-readable string.
    /// </summary>
    public static string FormatDuration(int totalSeconds)
    {
        var time = TimeSpan.FromSeconds(totalSeconds);
        return FormatTime(time);
    }

    /// <summary>
    /// Formats a TimeSpan appropriately based on duration.
    /// </summary>
    private static string FormatTime(TimeSpan time)
    {
        if (time.TotalHours >= 1)
        {
            return $"{(int)time.TotalHours}:{time.Minutes:D2}:{time.Seconds:D2}";
        }
        return $"{(int)time.TotalMinutes}:{time.Seconds:D2}";
    }
}
