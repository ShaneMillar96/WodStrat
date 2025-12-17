using WodStrat.Services.Dtos;

namespace WodStrat.Services.Interfaces;

/// <summary>
/// Service interface for workout CRUD operations.
/// Handles workout persistence, retrieval, and user ownership.
/// </summary>
public interface IWorkoutService
{
    #region Workout CRUD Operations

    /// <summary>
    /// Creates a new workout from parsed data for the current user.
    /// </summary>
    /// <param name="dto">The workout creation data (parsed result or manual input).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created workout with generated ID, or null if user not authenticated.</returns>
    Task<WorkoutDto?> CreateWorkoutAsync(CreateWorkoutDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a workout by ID.
    /// </summary>
    /// <param name="id">The workout's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The workout if found and not deleted; otherwise null.</returns>
    Task<WorkoutDto?> GetWorkoutByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all workouts for the current authenticated user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of user's workouts ordered by creation date (newest first).</returns>
    Task<IReadOnlyList<WorkoutDto>> GetCurrentUserWorkoutsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves workouts for a specific user by user ID.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of user's workouts ordered by creation date (newest first).</returns>
    Task<IReadOnlyList<WorkoutDto>> GetUserWorkoutsAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing workout.
    /// </summary>
    /// <param name="id">The workout's unique identifier.</param>
    /// <param name="dto">The workout update data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated workout if found and authorized; otherwise null.</returns>
    Task<WorkoutDto?> UpdateWorkoutAsync(int id, UpdateWorkoutDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a workout.
    /// </summary>
    /// <param name="id">The workout's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if found and deleted; false if not found or unauthorized.</returns>
    Task<bool> DeleteWorkoutAsync(int id, CancellationToken cancellationToken = default);

    #endregion

    #region Ownership Validation

    /// <summary>
    /// Validates that the specified workout belongs to the current user.
    /// </summary>
    /// <param name="workoutId">The workout's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the workout belongs to the current user; otherwise false.</returns>
    Task<bool> ValidateOwnershipAsync(int workoutId, CancellationToken cancellationToken = default);

    #endregion
}
