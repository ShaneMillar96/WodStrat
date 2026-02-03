using WodStrat.Dal.Enums;
using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;

namespace WodStrat.Services.Interfaces;

/// <summary>
/// Service interface for calculating personalized pacing recommendations
/// based on athlete benchmark performance relative to population averages.
/// </summary>
public interface IPacingService
{
    /// <summary>
    /// Calculates pacing recommendations for all movements in a workout.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="workoutId">The workout's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Complete workout pacing result with per-movement guidance, or null if workout not found.</returns>
    Task<WorkoutPacingResultDto?> CalculateWorkoutPacingAsync(
        int athleteId,
        int workoutId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates pacing recommendation for a single movement.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="movementDefinitionId">The movement definition ID.</param>
    /// <param name="repCount">Number of reps to perform.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Movement pacing DTO with guidance, or null if movement not found.</returns>
    Task<MovementPacingDto?> CalculateMovementPacingAsync(
        int athleteId,
        int movementDefinitionId,
        int repCount,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines the pacing level for an athlete based on a specific benchmark.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="benchmarkDefinitionId">The benchmark definition ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The calculated pacing level (defaults to Moderate if insufficient data).</returns>
    Task<PacingLevel> DetermineAthletePacingLevelAsync(
        int athleteId,
        int benchmarkDefinitionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates the athlete's percentile within the population for a benchmark.
    /// Handles both "higher is better" (Reps, Weight) and "lower is better" (Time, Pace) metrics.
    /// </summary>
    /// <param name="athleteValue">The athlete's benchmark value.</param>
    /// <param name="populationData">Population percentile reference data.</param>
    /// <param name="metricType">The metric type (determines if higher/lower is better).</param>
    /// <returns>Percentile value (0-100).</returns>
    decimal CalculateAthletePercentile(
        decimal athleteValue,
        PopulationBenchmarkPercentile populationData,
        BenchmarkMetricType metricType);

    /// <summary>
    /// Calculates recommended set breakdown for given total reps and pacing level.
    /// </summary>
    /// <param name="totalReps">Total number of reps to perform.</param>
    /// <param name="pacingLevel">The determined pacing level.</param>
    /// <returns>Array of rep counts per set.</returns>
    int[] CalculateSetBreakdown(int totalReps, PacingLevel pacingLevel);

    /// <summary>
    /// Generates human-readable guidance text for a movement.
    /// </summary>
    /// <param name="totalReps">Total number of reps.</param>
    /// <param name="pacingLevel">The pacing level.</param>
    /// <param name="setBreakdown">The calculated set breakdown.</param>
    /// <param name="movementName">Display name of the movement.</param>
    /// <returns>Guidance text string.</returns>
    string GenerateGuidanceText(
        int totalReps,
        PacingLevel pacingLevel,
        int[] setBreakdown,
        string movementName);

    /// <summary>
    /// Calculates workout pacing for the current authenticated user.
    /// </summary>
    /// <param name="workoutId">The workout's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Workout pacing result, or null if user has no athlete profile or workout not found.</returns>
    Task<WorkoutPacingResultDto?> CalculateCurrentUserWorkoutPacingAsync(
        int workoutId,
        CancellationToken cancellationToken = default);
}
