using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;

namespace WodStrat.Services.Extensions;

/// <summary>
/// Extension methods for mapping movement context data.
/// </summary>
public static class MovementContextMappingExtensions
{
    /// <summary>
    /// Maps a MovementDefinition to MovementContextDto.
    /// </summary>
    /// <param name="movement">The movement definition entity.</param>
    /// <param name="benchmarkUsed">Name of the benchmark used for analysis.</param>
    /// <param name="athletePercentile">Athlete's percentile for this movement.</param>
    /// <param name="hasPopulationData">Whether population data was available.</param>
    /// <param name="hasAthleteBenchmark">Whether the athlete has recorded the benchmark.</param>
    /// <returns>A new MovementContextDto instance.</returns>
    public static MovementContextDto ToMovementContextDto(
        this MovementDefinition movement,
        string benchmarkUsed,
        decimal? athletePercentile,
        bool hasPopulationData,
        bool hasAthleteBenchmark)
    {
        return new MovementContextDto
        {
            MovementDefinitionId = movement.Id,
            MovementName = movement.DisplayName,
            CanonicalName = movement.CanonicalName,
            Category = movement.Category.ToString(),
            IsBodyweight = movement.IsBodyweight,
            BenchmarkUsed = benchmarkUsed,
            AthletePercentile = athletePercentile,
            HasPopulationData = hasPopulationData,
            HasAthleteBenchmark = hasAthleteBenchmark
        };
    }

    /// <summary>
    /// Maps a Workout to WorkoutContextDto.
    /// </summary>
    /// <param name="workout">The workout entity.</param>
    /// <param name="movementContexts">The list of movement contexts.</param>
    /// <returns>A new WorkoutContextDto instance.</returns>
    public static WorkoutContextDto ToWorkoutContextDto(
        this Workout workout,
        IReadOnlyList<MovementContextDto> movementContexts)
    {
        return new WorkoutContextDto
        {
            WorkoutId = workout.Id,
            WorkoutName = workout.Name ?? "Unnamed Workout",
            WorkoutType = workout.WorkoutType.ToString(),
            TimeCapSeconds = workout.TimeCapSeconds,
            RoundCount = workout.RoundCount,
            Movements = movementContexts,
            CalculatedAt = DateTime.UtcNow
        };
    }
}
