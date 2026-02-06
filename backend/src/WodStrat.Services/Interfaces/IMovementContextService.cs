using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;

namespace WodStrat.Services.Interfaces;

/// <summary>
/// Service interface for building unified movement context data
/// that is shared across all strategy services.
/// </summary>
public interface IMovementContextService
{
    /// <summary>
    /// Builds movement contexts for all movements in a workout.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="workoutId">The workout's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of movement contexts, or empty if workout not found.</returns>
    Task<IReadOnlyList<MovementContextDto>> BuildMovementContextsAsync(
        int athleteId,
        int workoutId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds workout context including all movement contexts.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="workoutId">The workout's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Workout context with movement data, or null if workout not found.</returns>
    Task<WorkoutContextDto?> BuildWorkoutContextAsync(
        int athleteId,
        int workoutId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds a single movement context from a movement definition.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="movementDefinition">The movement definition entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Movement context for the specified movement.</returns>
    Task<MovementContextDto> BuildMovementContextAsync(
        int athleteId,
        MovementDefinition movementDefinition,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds workout context for the current authenticated user.
    /// </summary>
    /// <param name="workoutId">The workout's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Workout context, or null if user has no athlete profile or workout not found.</returns>
    Task<WorkoutContextDto?> BuildCurrentUserWorkoutContextAsync(
        int workoutId,
        CancellationToken cancellationToken = default);
}
