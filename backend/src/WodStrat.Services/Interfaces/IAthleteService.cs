using WodStrat.Services.Dtos;

namespace WodStrat.Services.Interfaces;

/// <summary>
/// Service interface for athlete profile management operations.
/// </summary>
public interface IAthleteService
{
    /// <summary>
    /// Retrieves an athlete profile by ID.
    /// </summary>
    /// <param name="id">The athlete's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The athlete DTO if found and not deleted; otherwise null.</returns>
    Task<AthleteDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an athlete profile by user ID (for authenticated user lookup).
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The athlete DTO if found and not deleted; otherwise null.</returns>
    Task<AthleteDto?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new athlete profile.
    /// </summary>
    /// <param name="dto">The athlete creation data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created athlete DTO with generated ID.</returns>
    Task<AthleteDto> CreateAsync(CreateAthleteDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing athlete profile.
    /// </summary>
    /// <param name="id">The athlete's unique identifier.</param>
    /// <param name="dto">The athlete update data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated athlete DTO if found; otherwise null.</returns>
    Task<AthleteDto?> UpdateAsync(int id, UpdateAthleteDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes an athlete profile.
    /// </summary>
    /// <param name="id">The athlete's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the athlete was found and deleted; otherwise false.</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
