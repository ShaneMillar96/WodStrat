using Microsoft.EntityFrameworkCore;
using WodStrat.Dal.Enums;
using WodStrat.Dal.Interfaces;
using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;
using WodStrat.Services.Extensions;
using WodStrat.Services.Interfaces;

namespace WodStrat.Services.Services;

/// <summary>
/// Service implementation for movement definition read operations.
/// Provides cached access to the canonical movement library.
/// </summary>
public class MovementDefinitionService : IMovementDefinitionService
{
    private readonly IWodStratDatabase _database;

    // Cached lookup dictionaries (lazy-loaded, thread-safe)
    private Dictionary<string, int>? _aliasToIdCache;
    private Dictionary<int, MovementDefinition>? _idToMovementCache;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    public MovementDefinitionService(IWodStratDatabase database)
    {
        _database = database;
    }

    #region Existing Methods (unchanged signatures, enhanced with caching)

    /// <inheritdoc />
    public async Task<IReadOnlyList<MovementDefinitionDto>> GetAllActiveMovementsAsync(CancellationToken cancellationToken = default)
    {
        var movements = await _database.Get<MovementDefinition>()
            .Include(m => m.Aliases)
            .Where(m => m.IsActive)
            .OrderBy(m => m.Category)
            .ThenBy(m => m.DisplayName)
            .ToListAsync(cancellationToken);

        return movements.Select(m => m.ToDto()).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MovementDefinitionDto>> GetMovementsByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<MovementCategory>(category, ignoreCase: true, out var categoryEnum))
        {
            return Array.Empty<MovementDefinitionDto>();
        }

        return await GetMovementsByCategoryAsync(categoryEnum, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<MovementDefinitionDto?> GetMovementByCanonicalNameAsync(string canonicalName, CancellationToken cancellationToken = default)
    {
        var normalizedName = canonicalName.ToLowerInvariant().Trim();

        var movement = await _database.Get<MovementDefinition>()
            .Include(m => m.Aliases)
            .Where(m => m.IsActive && m.CanonicalName == normalizedName)
            .FirstOrDefaultAsync(cancellationToken);

        return movement?.ToDto();
    }

    /// <inheritdoc />
    public async Task<MovementDefinitionDto?> FindMovementByAliasAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var normalizedSearch = searchTerm.ToLowerInvariant().Trim();

        // First try to find by alias
        var alias = await _database.Get<MovementAlias>()
            .Include(a => a.MovementDefinition)
                .ThenInclude(m => m.Aliases)
            .Where(a => a.Alias.ToLower() == normalizedSearch && a.MovementDefinition.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (alias != null)
        {
            return alias.MovementDefinition.ToDto();
        }

        // Fall back to canonical name or display name
        var movement = await _database.Get<MovementDefinition>()
            .Include(m => m.Aliases)
            .Where(m => m.IsActive &&
                (m.CanonicalName == normalizedSearch ||
                 m.DisplayName.ToLower() == normalizedSearch))
            .FirstOrDefaultAsync(cancellationToken);

        return movement?.ToDto();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, int>> GetAliasLookupAsync(CancellationToken cancellationToken = default)
    {
        await EnsureCacheLoadedAsync(cancellationToken);
        return _aliasToIdCache!;
    }

    #endregion

    #region New Methods (WOD-12)

    /// <inheritdoc />
    public async Task<string?> NormalizeMovementNameAsync(string input, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var normalizedInput = NormalizeSearchTerm(input);

        await EnsureCacheLoadedAsync(cancellationToken);

        // Try direct lookup in alias cache
        if (_aliasToIdCache!.TryGetValue(normalizedInput, out var movementId))
        {
            if (_idToMovementCache!.TryGetValue(movementId, out var movement))
            {
                return movement.CanonicalName;
            }
        }

        // Try without common variations (hyphens, underscores, spaces)
        var variations = GenerateSearchVariations(normalizedInput);
        foreach (var variation in variations)
        {
            if (_aliasToIdCache.TryGetValue(variation, out var variantId))
            {
                if (_idToMovementCache!.TryGetValue(variantId, out var movement))
                {
                    return movement.CanonicalName;
                }
            }
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MovementDefinitionDto>> SearchMovementsAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<MovementDefinitionDto>();
        }

        var normalizedQuery = NormalizeSearchTerm(query);

        await EnsureCacheLoadedAsync(cancellationToken);

        // Score and rank matches
        var scoredMatches = new List<(MovementDefinition Movement, int Score)>();

        foreach (var movement in _idToMovementCache!.Values)
        {
            var score = CalculateMatchScore(movement, normalizedQuery);
            if (score > 0)
            {
                scoredMatches.Add((movement, score));
            }
        }

        // Order by score (descending), then by display name
        return scoredMatches
            .OrderByDescending(m => m.Score)
            .ThenBy(m => m.Movement.DisplayName)
            .Select(m => m.Movement.ToDto())
            .ToList();
    }

    /// <inheritdoc />
    public async Task<MovementDefinitionDto?> GetMovementByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await EnsureCacheLoadedAsync(cancellationToken);

        if (_idToMovementCache!.TryGetValue(id, out var movement))
        {
            return movement.ToDto();
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MovementDefinitionDto>> GetMovementsByCategoryAsync(MovementCategory category, CancellationToken cancellationToken = default)
    {
        var movements = await _database.Get<MovementDefinition>()
            .Include(m => m.Aliases)
            .Where(m => m.IsActive && m.Category == category)
            .OrderBy(m => m.DisplayName)
            .ToListAsync(cancellationToken);

        return movements.Select(m => m.ToDto()).ToList();
    }

    /// <inheritdoc />
    public void InvalidateCache()
    {
        _cacheLock.Wait();
        try
        {
            _aliasToIdCache = null;
            _idToMovementCache = null;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    #endregion

    #region Private Cache Methods

    /// <summary>
    /// Ensures the cache is loaded, with thread-safe lazy initialization.
    /// </summary>
    private async Task EnsureCacheLoadedAsync(CancellationToken cancellationToken)
    {
        if (_aliasToIdCache != null && _idToMovementCache != null)
        {
            return;
        }

        await _cacheLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (_aliasToIdCache != null && _idToMovementCache != null)
            {
                return;
            }

            // Load all active movements with aliases
            var movements = await _database.Get<MovementDefinition>()
                .Include(m => m.Aliases)
                .Where(m => m.IsActive)
                .ToListAsync(cancellationToken);

            // Build ID to movement cache
            _idToMovementCache = movements.ToDictionary(m => m.Id);

            // Build alias to ID cache (case-insensitive)
            var aliasCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var movement in movements)
            {
                // Add canonical name
                aliasCache.TryAdd(movement.CanonicalName, movement.Id);

                // Add display name (original lowercase for exact match)
                aliasCache.TryAdd(movement.DisplayName.ToLowerInvariant(), movement.Id);

                // Add display name (normalized for flexible matching)
                aliasCache.TryAdd(NormalizeSearchTerm(movement.DisplayName), movement.Id);

                // Add all aliases
                foreach (var alias in movement.Aliases)
                {
                    aliasCache.TryAdd(alias.Alias.ToLowerInvariant(), movement.Id);
                    aliasCache.TryAdd(NormalizeSearchTerm(alias.Alias), movement.Id);
                }
            }

            _aliasToIdCache = aliasCache;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    /// <summary>
    /// Normalizes a search term for consistent matching.
    /// </summary>
    private static string NormalizeSearchTerm(string term)
    {
        return term
            .ToLowerInvariant()
            .Replace("-", "")
            .Replace("_", "")
            .Replace(" ", "")
            .Replace("'", "")
            .Trim();
    }

    /// <summary>
    /// Generates common variations of a search term.
    /// </summary>
    private static IEnumerable<string> GenerateSearchVariations(string normalizedTerm)
    {
        yield return normalizedTerm;

        // Try with trailing 's' removed (plurals)
        if (normalizedTerm.EndsWith("s") && normalizedTerm.Length > 1)
        {
            yield return normalizedTerm[..^1];
        }

        // Try with trailing 's' added
        yield return normalizedTerm + "s";
    }

    /// <summary>
    /// Calculates a relevance score for a movement against a search query.
    /// Higher scores indicate better matches.
    /// </summary>
    private static int CalculateMatchScore(MovementDefinition movement, string normalizedQuery)
    {
        var score = 0;
        var canonicalNormalized = NormalizeSearchTerm(movement.CanonicalName);
        var displayNormalized = NormalizeSearchTerm(movement.DisplayName);

        // Exact match on canonical name (highest priority)
        if (canonicalNormalized == normalizedQuery)
        {
            return 100;
        }

        // Exact match on display name
        if (displayNormalized == normalizedQuery)
        {
            return 95;
        }

        // Exact match on alias
        foreach (var alias in movement.Aliases)
        {
            if (NormalizeSearchTerm(alias.Alias) == normalizedQuery)
            {
                return 90;
            }
        }

        // Canonical name starts with query
        if (canonicalNormalized.StartsWith(normalizedQuery))
        {
            score = Math.Max(score, 70);
        }

        // Display name starts with query
        if (displayNormalized.StartsWith(normalizedQuery))
        {
            score = Math.Max(score, 65);
        }

        // Alias starts with query
        foreach (var alias in movement.Aliases)
        {
            if (NormalizeSearchTerm(alias.Alias).StartsWith(normalizedQuery))
            {
                score = Math.Max(score, 60);
                break;
            }
        }

        // Canonical name contains query
        if (canonicalNormalized.Contains(normalizedQuery))
        {
            score = Math.Max(score, 40);
        }

        // Display name contains query
        if (displayNormalized.Contains(normalizedQuery))
        {
            score = Math.Max(score, 35);
        }

        // Alias contains query
        foreach (var alias in movement.Aliases)
        {
            if (NormalizeSearchTerm(alias.Alias).Contains(normalizedQuery))
            {
                score = Math.Max(score, 30);
                break;
            }
        }

        // Query contains canonical name (partial match)
        if (normalizedQuery.Contains(canonicalNormalized) && canonicalNormalized.Length > 2)
        {
            score = Math.Max(score, 20);
        }

        return score;
    }

    #endregion
}
