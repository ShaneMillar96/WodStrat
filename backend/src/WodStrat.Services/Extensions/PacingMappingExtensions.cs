using WodStrat.Dal.Enums;
using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;

namespace WodStrat.Services.Extensions;

/// <summary>
/// Extension methods for mapping pacing-related data.
/// </summary>
public static class PacingMappingExtensions
{
    /// <summary>
    /// Converts a PacingLevel enum to its string representation.
    /// </summary>
    /// <param name="level">The pacing level enum value.</param>
    /// <returns>The display string for the pacing level.</returns>
    public static string ToDisplayString(this PacingLevel level) => level switch
    {
        PacingLevel.Light => "Light",
        PacingLevel.Moderate => "Moderate",
        PacingLevel.Heavy => "Heavy",
        _ => "Unknown"
    };

    /// <summary>
    /// Creates a MovementPacingDto from calculation results.
    /// </summary>
    /// <param name="movement">The movement definition entity.</param>
    /// <param name="pacingLevel">The calculated pacing level.</param>
    /// <param name="percentile">The athlete's percentile ranking.</param>
    /// <param name="setBreakdown">The recommended set breakdown.</param>
    /// <param name="guidanceText">The human-readable guidance text.</param>
    /// <param name="benchmarkName">The name of the benchmark used.</param>
    /// <param name="hasPopulationData">Whether population data was available.</param>
    /// <param name="hasAthleteBenchmark">Whether the athlete has recorded the benchmark.</param>
    /// <returns>A new MovementPacingDto instance.</returns>
    public static MovementPacingDto ToMovementPacingDto(
        this MovementDefinition movement,
        PacingLevel pacingLevel,
        decimal percentile,
        int[] setBreakdown,
        string guidanceText,
        string benchmarkName,
        bool hasPopulationData,
        bool hasAthleteBenchmark)
    {
        return new MovementPacingDto
        {
            MovementDefinitionId = movement.Id,
            MovementName = movement.DisplayName,
            PacingLevel = pacingLevel.ToDisplayString(),
            AthletePercentile = percentile,
            GuidanceText = guidanceText,
            RecommendedSets = setBreakdown,
            BenchmarkUsed = benchmarkName,
            HasPopulationData = hasPopulationData,
            HasAthleteBenchmark = hasAthleteBenchmark
        };
    }

    /// <summary>
    /// Creates a WorkoutPacingResultDto from calculation results.
    /// </summary>
    /// <param name="workout">The workout entity.</param>
    /// <param name="movementPacing">The list of movement pacing DTOs.</param>
    /// <param name="strategyNotes">The overall strategy notes.</param>
    /// <returns>A new WorkoutPacingResultDto instance.</returns>
    public static WorkoutPacingResultDto ToWorkoutPacingResultDto(
        this Workout workout,
        IReadOnlyList<MovementPacingDto> movementPacing,
        string strategyNotes)
    {
        var distribution = new PacingDistributionDto
        {
            HeavyCount = movementPacing.Count(m => m.PacingLevel == "Heavy"),
            ModerateCount = movementPacing.Count(m => m.PacingLevel == "Moderate"),
            LightCount = movementPacing.Count(m => m.PacingLevel == "Light"),
            TotalMovements = movementPacing.Count,
            IncompleteDataCount = movementPacing.Count(m => !m.HasPopulationData || !m.HasAthleteBenchmark)
        };

        return new WorkoutPacingResultDto
        {
            WorkoutId = workout.Id,
            WorkoutName = workout.Name ?? "Unnamed Workout",
            WorkoutType = workout.WorkoutType.ToString(),
            MovementPacing = movementPacing,
            OverallStrategyNotes = strategyNotes,
            CalculatedAt = DateTime.UtcNow,
            Distribution = distribution,
            IsComplete = distribution.IncompleteDataCount == 0
        };
    }
}
