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
/// </summary>
public class MovementDefinitionService : IMovementDefinitionService
{
    private readonly IWodStratDatabase _database;

    public MovementDefinitionService(IWodStratDatabase database)
    {
        _database = database;
    }

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

        var movements = await _database.Get<MovementDefinition>()
            .Include(m => m.Aliases)
            .Where(m => m.IsActive && m.Category == categoryEnum)
            .OrderBy(m => m.DisplayName)
            .ToListAsync(cancellationToken);

        return movements.Select(m => m.ToDto()).ToList();
    }

    /// <inheritdoc />
    public async Task<MovementDefinitionDto?> GetMovementByCanonicalNameAsync(string canonicalName, CancellationToken cancellationToken = default)
    {
        var movement = await _database.Get<MovementDefinition>()
            .Include(m => m.Aliases)
            .Where(m => m.IsActive && m.CanonicalName == canonicalName.ToLowerInvariant())
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
        // Get all aliases
        var aliases = await _database.Get<MovementAlias>()
            .Where(a => a.MovementDefinition.IsActive)
            .Select(a => new { Alias = a.Alias.ToLower(), a.MovementDefinitionId })
            .ToListAsync(cancellationToken);

        // Get canonical names and display names
        var movements = await _database.Get<MovementDefinition>()
            .Where(m => m.IsActive)
            .Select(m => new { m.Id, m.CanonicalName, m.DisplayName })
            .ToListAsync(cancellationToken);

        var lookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // Add aliases
        foreach (var alias in aliases)
        {
            lookup.TryAdd(alias.Alias, alias.MovementDefinitionId);
        }

        // Add canonical names and display names
        foreach (var movement in movements)
        {
            lookup.TryAdd(movement.CanonicalName, movement.Id);
            lookup.TryAdd(movement.DisplayName.ToLower(), movement.Id);
        }

        return lookup;
    }
}
