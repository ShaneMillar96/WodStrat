using WodStrat.Services.Dtos;

namespace WodStrat.Services.Interfaces;

/// <summary>
/// Service interface for calculating comprehensive workout strategy insights
/// by combining pacing, volume load, and time estimate analyses.
/// </summary>
public interface IStrategyInsightsService
{
    /// <summary>
    /// Calculates comprehensive strategy insights for a workout based on athlete benchmarks.
    /// Combines pacing, volume load, and time estimate analyses into actionable recommendations.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="workoutId">The workout's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Strategy insights result, or null if workout not found.</returns>
    Task<StrategyInsightsResultDto?> CalculateStrategyInsightsAsync(
        int athleteId,
        int workoutId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates strategy insights for the current authenticated user.
    /// </summary>
    /// <param name="workoutId">The workout's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Strategy insights result, or null if user has no athlete profile or workout not found.</returns>
    Task<StrategyInsightsResultDto?> CalculateCurrentUserStrategyInsightsAsync(
        int workoutId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates the difficulty score for a workout based on combined service outputs.
    /// Pure calculation method (no I/O).
    /// </summary>
    /// <param name="pacingResult">Pacing analysis result.</param>
    /// <param name="volumeResult">Volume load analysis result.</param>
    /// <param name="timeResult">Time estimate result.</param>
    /// <param name="experienceLevel">Athlete's experience level.</param>
    /// <returns>Difficulty score DTO with breakdown.</returns>
    DifficultyScoreDto CalculateDifficultyScore(
        WorkoutPacingResultDto pacingResult,
        WorkoutVolumeLoadResultDto volumeResult,
        TimeEstimateResultDto timeResult,
        string experienceLevel);

    /// <summary>
    /// Identifies key focus movements based on pacing and volume analysis.
    /// Pure calculation method (no I/O).
    /// </summary>
    /// <param name="pacingResult">Pacing analysis result.</param>
    /// <param name="volumeResult">Volume load analysis result.</param>
    /// <param name="maxMovements">Maximum number of focus movements to return (default 3).</param>
    /// <returns>List of key focus movements ordered by priority.</returns>
    IReadOnlyList<KeyFocusMovementDto> IdentifyKeyFocusMovements(
        WorkoutPacingResultDto pacingResult,
        WorkoutVolumeLoadResultDto volumeResult,
        int maxMovements = 3);

    /// <summary>
    /// Generates risk alerts based on combined analysis data.
    /// Pure calculation method (no I/O).
    /// </summary>
    /// <param name="pacingResult">Pacing analysis result.</param>
    /// <param name="volumeResult">Volume load analysis result.</param>
    /// <param name="timeResult">Time estimate result.</param>
    /// <param name="difficultyScore">Calculated difficulty score.</param>
    /// <param name="timeCapSeconds">Optional time cap for the workout.</param>
    /// <returns>List of risk alerts.</returns>
    IReadOnlyList<RiskAlertDto> GenerateRiskAlerts(
        WorkoutPacingResultDto pacingResult,
        WorkoutVolumeLoadResultDto volumeResult,
        TimeEstimateResultDto timeResult,
        int difficultyScore,
        int? timeCapSeconds);

    /// <summary>
    /// Calculates strategy confidence based on benchmark coverage across services.
    /// Pure calculation method (no I/O).
    /// </summary>
    /// <param name="pacingResult">Pacing analysis result.</param>
    /// <param name="volumeResult">Volume load analysis result.</param>
    /// <param name="timeResult">Time estimate result.</param>
    /// <returns>Strategy confidence DTO.</returns>
    StrategyConfidenceDto CalculateStrategyConfidence(
        WorkoutPacingResultDto pacingResult,
        WorkoutVolumeLoadResultDto volumeResult,
        TimeEstimateResultDto timeResult);
}
