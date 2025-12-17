using WodStrat.Dal.Enums;
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

    /// <summary>
    /// Normalizes a movement name/alias to its canonical form.
    /// Performs case-insensitive lookup against aliases, canonical names, and display names.
    /// </summary>
    /// <param name="input">The movement name or alias to normalize (e.g., "T2B", "toes-to-bar", "TTB").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The canonical name if found (e.g., "toes_to_bar"); otherwise null.</returns>
    Task<string?> NormalizeMovementNameAsync(string input, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for movements matching a query string.
    /// Performs fuzzy matching against display names, canonical names, and aliases.
    /// </summary>
    /// <param name="query">The search query (case-insensitive, partial match supported).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of matching movement definitions ordered by relevance.</returns>
    Task<IReadOnlyList<MovementDefinitionDto>> SearchMovementsAsync(string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a movement definition by its database ID.
    /// </summary>
    /// <param name="id">The movement definition ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The movement definition if found and active; otherwise null.</returns>
    Task<MovementDefinitionDto?> GetMovementByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves movements filtered by category enum.
    /// </summary>
    /// <param name="category">The movement category enum value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of movement definitions in the specified category.</returns>
    Task<IReadOnlyList<MovementDefinitionDto>> GetMovementsByCategoryAsync(MovementCategory category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates the in-memory cache, forcing a reload on next access.
    /// Call this after movement data is modified.
    /// </summary>
    void InvalidateCache();
}
