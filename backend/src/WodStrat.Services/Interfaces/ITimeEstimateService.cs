using WodStrat.Dal.Enums;
using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;

namespace WodStrat.Services.Interfaces;

/// <summary>
/// Service interface for calculating personalized time estimates and feasibility
/// assessments for workouts based on athlete benchmark data and performance characteristics.
/// </summary>
public interface ITimeEstimateService
{
    /// <summary>
    /// Main entry point - routes to appropriate workout-type-specific method.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="workoutId">The workout's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Complete time estimate result, or null if workout not found.</returns>
    Task<TimeEstimateResultDto?> EstimateWorkoutTimeAsync(
        int athleteId,
        int workoutId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// For Time workouts - estimate completion time range.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="workout">The workout entity with movements loaded.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Time estimate result for the ForTime workout.</returns>
    Task<TimeEstimateResultDto> EstimateForTimeWorkoutAsync(
        int athleteId,
        Workout workout,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// AMRAP workouts - estimate rounds + reps range.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="workout">The workout entity with movements loaded.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>AMRAP estimate result with rounds and reps range.</returns>
    Task<TimeEstimateResultDto> EstimateAmrapWorkoutAsync(
        int athleteId,
        Workout workout,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// EMOM workouts - per-minute feasibility check.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="workout">The workout entity with movements loaded.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of feasibility analysis for each minute.</returns>
    Task<List<EmomFeasibilityDto>> CheckEmomFeasibilityAsync(
        int athleteId,
        Workout workout,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Interval workouts - per-interval performance estimate.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="workout">The workout entity with movements loaded.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Interval workout estimate result.</returns>
    Task<TimeEstimateResultDto> EstimateIntervalWorkoutAsync(
        int athleteId,
        Workout workout,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates time range based on athlete percentile and experience level.
    /// Pure calculation method (no I/O).
    /// </summary>
    /// <param name="baseTimeSeconds">Base estimated time in seconds.</param>
    /// <param name="athletePercentile">Athlete's percentile (0-100).</param>
    /// <param name="experience">Athlete's experience level.</param>
    /// <param name="benchmarkCoveragePercent">Percentage of movements with benchmark data.</param>
    /// <returns>Tuple of (minSeconds, maxSeconds).</returns>
    (int minSeconds, int maxSeconds) CalculateTimeRange(
        int baseTimeSeconds,
        decimal athletePercentile,
        ExperienceLevel experience,
        int benchmarkCoveragePercent);

    /// <summary>
    /// Calculates recommended set breakdown for given total reps and pacing level.
    /// </summary>
    /// <param name="movements">List of workout movements.</param>
    /// <param name="overallPacing">The overall pacing level for the workout.</param>
    /// <param name="movementPacingLevels">Dictionary mapping movement definition ID to pacing level.</param>
    /// <returns>List of rest recommendations for each movement.</returns>
    List<RestRecommendationDto> CalculateRestRecommendations(
        List<WorkoutMovement> movements,
        PacingLevel overallPacing,
        Dictionary<int, PacingLevel> movementPacingLevels);

    /// <summary>
    /// Formats a time range as human-readable string (e.g., "8:30 - 10:15").
    /// </summary>
    /// <param name="minSeconds">Minimum time in seconds.</param>
    /// <param name="maxSeconds">Maximum time in seconds.</param>
    /// <returns>Formatted time range string.</returns>
    string FormatTimeRange(int minSeconds, int maxSeconds);

    /// <summary>
    /// Formats an AMRAP range as human-readable string (e.g., "4+12 to 5+8 rounds").
    /// </summary>
    /// <param name="minRounds">Minimum rounds.</param>
    /// <param name="minReps">Additional reps beyond min rounds.</param>
    /// <param name="maxRounds">Maximum rounds.</param>
    /// <param name="maxReps">Additional reps beyond max rounds.</param>
    /// <param name="repsPerRound">Total reps in one complete round.</param>
    /// <returns>Formatted AMRAP range string.</returns>
    string FormatAmrapRange(int minRounds, int minReps, int maxRounds, int maxReps, int repsPerRound);

    /// <summary>
    /// Calculates workout time estimate for the current authenticated user.
    /// </summary>
    /// <param name="workoutId">The workout's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Time estimate result, or null if user has no athlete profile or workout not found.</returns>
    Task<TimeEstimateResultDto?> EstimateCurrentUserWorkoutTimeAsync(
        int workoutId,
        CancellationToken cancellationToken = default);
}
