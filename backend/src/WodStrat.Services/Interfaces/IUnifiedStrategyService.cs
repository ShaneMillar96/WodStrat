using WodStrat.Services.Dtos;

namespace WodStrat.Services.Interfaces;

/// <summary>
/// Service interface for calculating unified strategy results
/// by orchestrating all strategy services with shared context.
/// </summary>
public interface IUnifiedStrategyService
{
    /// <summary>
    /// Calculates unified strategy with shared context for a workout.
    /// This combines pacing, volume load, time estimate, and strategy insights
    /// into a single response with deduplicated movement data.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="workoutId">The workout's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Unified strategy result, or null if workout not found.</returns>
    Task<UnifiedStrategyResultDto?> CalculateUnifiedStrategyAsync(
        int athleteId,
        int workoutId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates unified strategy for the current authenticated user.
    /// </summary>
    /// <param name="workoutId">The workout's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Unified strategy result, or null if user has no athlete profile or workout not found.</returns>
    Task<UnifiedStrategyResultDto?> CalculateCurrentUserUnifiedStrategyAsync(
        int workoutId,
        CancellationToken cancellationToken = default);
}
