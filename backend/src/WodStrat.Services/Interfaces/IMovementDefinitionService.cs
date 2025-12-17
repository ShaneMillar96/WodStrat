using WodStrat.Services.Dtos;

namespace WodStrat.Services.Interfaces;

/// <summary>
/// Service interface for movement definition read operations.
/// Provides access to the canonical movement library and alias lookups.
/// </summary>
public interface IMovementDefinitionService
{
    /// <summary>
    /// Retrieves all active movement definitions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of active movement definitions ordered by category and name.</returns>
    Task<IReadOnlyList<MovementDefinitionDto>> GetAllActiveMovementsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves active movement definitions filtered by category.
    /// </summary>
    /// <param name="category">The category name to filter by (case-insensitive).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of movement definitions in the specified category.</returns>
    Task<IReadOnlyList<MovementDefinitionDto>> GetMovementsByCategoryAsync(string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a movement definition by its canonical name.
    /// </summary>
    /// <param name="canonicalName">The canonical name (e.g., "toes_to_bar").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The movement definition if found and active; otherwise null.</returns>
    Task<MovementDefinitionDto?> GetMovementByCanonicalNameAsync(string canonicalName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a movement definition by an alias or canonical name.
    /// Performs case-insensitive lookup against both aliases and canonical names.
    /// </summary>
    /// <param name="searchTerm">The alias or name to search for (e.g., "T2B", "toes-to-bar").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matched movement definition if found; otherwise null.</returns>
    Task<MovementDefinitionDto?> FindMovementByAliasAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all aliases for movement definitions (used for parsing optimization).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary mapping lowercase aliases to movement definition IDs.</returns>
    Task<IReadOnlyDictionary<string, int>> GetAliasLookupAsync(CancellationToken cancellationToken = default);
}
