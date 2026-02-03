using WodStrat.Dal.Enums;
using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;

namespace WodStrat.Services.Interfaces;

/// <summary>
/// Service interface for calculating personalized volume load analysis
/// based on athlete benchmark performance and experience level.
/// </summary>
public interface IVolumeLoadService
{
    /// <summary>
    /// Calculates volume load for all movements in a workout with personalized recommendations.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="workoutId">The workout's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Complete workout volume load result, or null if workout not found.</returns>
    Task<WorkoutVolumeLoadResultDto?> CalculateWorkoutVolumeLoadAsync(
        int athleteId,
        int workoutId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates volume load for a single workout movement.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="movement">The workout movement entity with loaded MovementDefinition.</param>
    /// <param name="rounds">Number of rounds for the workout.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Movement volume load DTO with recommendations.</returns>
    Task<MovementVolumeLoadDto> CalculateMovementVolumeLoadAsync(
        int athleteId,
        WorkoutMovement movement,
        int rounds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates workout volume load for the current authenticated user.
    /// </summary>
    /// <param name="workoutId">The workout's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Workout volume load result, or null if user has no athlete profile or workout not found.</returns>
    Task<WorkoutVolumeLoadResultDto?> CalculateCurrentUserWorkoutVolumeLoadAsync(
        int workoutId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pure calculation of volume load formula: Weight x Reps x Rounds.
    /// </summary>
    /// <param name="weight">Weight per repetition.</param>
    /// <param name="reps">Number of repetitions.</param>
    /// <param name="rounds">Number of rounds.</param>
    /// <returns>Calculated volume load.</returns>
    decimal CalculateVolumeLoad(decimal weight, int reps, int rounds);

    /// <summary>
    /// Classifies the load intensity based on percentage of athlete's 1RM.
    /// Thresholds adjusted by experience level.
    /// </summary>
    /// <param name="workoutWeight">Weight used in the workout.</param>
    /// <param name="athlete1RM">Athlete's 1 rep max for the relevant benchmark.</param>
    /// <param name="experience">Athlete's experience level.</param>
    /// <returns>Load classification: "High", "Moderate", or "Low".</returns>
    string ClassifyLoad(decimal workoutWeight, decimal athlete1RM, ExperienceLevel experience);

    /// <summary>
    /// Generates actionable recommendation text based on load classification and athlete percentile.
    /// </summary>
    /// <param name="loadClassification">The determined load classification.</param>
    /// <param name="athletePercentile">Athlete's percentile for the benchmark (0-100).</param>
    /// <param name="rxWeight">The prescribed RX weight.</param>
    /// <param name="movementName">Display name of the movement.</param>
    /// <returns>Human-readable tip/recommendation.</returns>
    string GenerateTip(string loadClassification, decimal? athletePercentile, decimal rxWeight, string movementName);

    /// <summary>
    /// Calculates recommended scaled weight based on athlete percentile and load classification.
    /// </summary>
    /// <param name="rxWeight">The prescribed RX weight.</param>
    /// <param name="athletePercentile">Athlete's percentile for the benchmark (0-100).</param>
    /// <param name="loadClassification">The determined load classification.</param>
    /// <returns>Recommended weight, or null if RX is appropriate.</returns>
    decimal? CalculateRecommendedWeight(decimal rxWeight, decimal? athletePercentile, string loadClassification);

    /// <summary>
    /// Formats volume load value with thousands separator and unit.
    /// </summary>
    /// <param name="volumeLoad">The calculated volume load.</param>
    /// <param name="unit">The weight unit (e.g., "kg").</param>
    /// <returns>Formatted string (e.g., "2,150 kg").</returns>
    string FormatVolumeLoad(decimal volumeLoad, string unit);
}
