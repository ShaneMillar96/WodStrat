using Microsoft.EntityFrameworkCore;
using WodStrat.Dal.Enums;
using WodStrat.Dal.Interfaces;
using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;
using WodStrat.Services.Extensions;
using WodStrat.Services.Interfaces;

namespace WodStrat.Services.Services;

/// <summary>
/// Service implementation for benchmark management operations.
/// </summary>
public class BenchmarkService : IBenchmarkService
{
    private const int MinimumBenchmarksRequired = 3;
    private readonly IWodStratDatabase _database;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAthleteService _athleteService;

    public BenchmarkService(
        IWodStratDatabase database,
        ICurrentUserService currentUserService,
        IAthleteService athleteService)
    {
        _database = database;
        _currentUserService = currentUserService;
        _athleteService = athleteService;
    }

    #region Benchmark Definitions

    /// <inheritdoc />
    public async Task<IReadOnlyList<BenchmarkDefinitionDto>> GetAllDefinitionsAsync(CancellationToken cancellationToken = default)
    {
        var definitions = await _database.Get<BenchmarkDefinition>()
            .Where(d => d.IsActive)
            .OrderBy(d => d.DisplayOrder)
            .ThenBy(d => d.Name)
            .ToListAsync(cancellationToken);

        return definitions.Select(d => d.ToDto()).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BenchmarkDefinitionDto>> GetDefinitionsByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<BenchmarkCategory>(category, ignoreCase: true, out var categoryEnum))
        {
            return Array.Empty<BenchmarkDefinitionDto>();
        }

        var definitions = await _database.Get<BenchmarkDefinition>()
            .Where(d => d.IsActive && d.Category == categoryEnum)
            .OrderBy(d => d.DisplayOrder)
            .ThenBy(d => d.Name)
            .ToListAsync(cancellationToken);

        return definitions.Select(d => d.ToDto()).ToList();
    }

    /// <inheritdoc />
    public async Task<BenchmarkDefinitionDto?> GetDefinitionBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var definition = await _database.Get<BenchmarkDefinition>()
            .Where(d => d.IsActive && d.Slug == slug.ToLowerInvariant())
            .FirstOrDefaultAsync(cancellationToken);

        return definition?.ToDto();
    }

    #endregion

    #region Athlete Benchmarks

    /// <inheritdoc />
    public async Task<IReadOnlyList<AthleteBenchmarkDto>> GetAthleteBenchmarksAsync(int athleteId, CancellationToken cancellationToken = default)
    {
        var benchmarks = await _database.Get<AthleteBenchmark>()
            .Include(ab => ab.BenchmarkDefinition)
            .Where(ab => ab.AthleteId == athleteId && !ab.IsDeleted)
            .OrderBy(ab => ab.BenchmarkDefinition.Category)
            .ThenBy(ab => ab.BenchmarkDefinition.DisplayOrder)
            .ThenBy(ab => ab.BenchmarkDefinition.Name)
            .ToListAsync(cancellationToken);

        return benchmarks.Select(ab => ab.ToDto()).ToList();
    }

    /// <inheritdoc />
    public async Task<AthleteBenchmarkDto?> GetAthleteBenchmarkByIdAsync(int athleteId, int benchmarkId, CancellationToken cancellationToken = default)
    {
        var benchmark = await _database.Get<AthleteBenchmark>()
            .Include(ab => ab.BenchmarkDefinition)
            .Where(ab => ab.Id == benchmarkId && ab.AthleteId == athleteId && !ab.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        return benchmark?.ToDto();
    }

    /// <inheritdoc />
    public async Task<BenchmarkSummaryDto> GetBenchmarkSummaryAsync(int athleteId, CancellationToken cancellationToken = default)
    {
        var benchmarks = await GetAthleteBenchmarksAsync(athleteId, cancellationToken);

        var benchmarksByCategory = benchmarks
            .GroupBy(b => b.BenchmarkCategory)
            .ToDictionary(g => g.Key, g => g.Count());

        return new BenchmarkSummaryDto
        {
            AthleteId = athleteId,
            TotalBenchmarks = benchmarks.Count,
            MeetsMinimumRequirement = benchmarks.Count >= MinimumBenchmarksRequired,
            MinimumRequired = MinimumBenchmarksRequired,
            BenchmarksByCategory = benchmarksByCategory,
            Benchmarks = benchmarks
        };
    }

    /// <inheritdoc />
    public async Task<(AthleteBenchmarkDto? Result, bool IsDuplicate)> RecordBenchmarkAsync(int athleteId, RecordBenchmarkDto dto, CancellationToken cancellationToken = default)
    {
        // Check for existing benchmark (duplicate check)
        var existingBenchmark = await _database.Get<AthleteBenchmark>()
            .Where(ab => ab.AthleteId == athleteId
                && ab.BenchmarkDefinitionId == dto.BenchmarkDefinitionId
                && !ab.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingBenchmark != null)
        {
            return (null, true); // Duplicate found
        }

        // Create new benchmark
        var entity = dto.ToEntity(athleteId);

        _database.Add(entity);
        await _database.SaveChangesAsync(cancellationToken);

        // Load the definition for the DTO
        var savedBenchmark = await _database.Get<AthleteBenchmark>()
            .Include(ab => ab.BenchmarkDefinition)
            .Where(ab => ab.Id == entity.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return (savedBenchmark?.ToDto(), false);
    }

    /// <inheritdoc />
    public async Task<AthleteBenchmarkDto?> UpdateBenchmarkAsync(int athleteId, int benchmarkId, UpdateBenchmarkDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _database.Get<AthleteBenchmark>()
            .Include(ab => ab.BenchmarkDefinition)
            .Where(ab => ab.Id == benchmarkId && ab.AthleteId == athleteId && !ab.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            return null;
        }

        dto.ApplyTo(entity);

        _database.Update(entity);
        await _database.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }

    /// <inheritdoc />
    public async Task<bool> DeleteBenchmarkAsync(int athleteId, int benchmarkId, CancellationToken cancellationToken = default)
    {
        var entity = await _database.Get<AthleteBenchmark>()
            .Where(ab => ab.Id == benchmarkId && ab.AthleteId == athleteId && !ab.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            return false;
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        _database.Update(entity);
        await _database.SaveChangesAsync(cancellationToken);

        return true;
    }

    #endregion

    #region Current User Operations

    /// <inheritdoc />
    public async Task<IReadOnlyList<AthleteBenchmarkDto>> GetCurrentUserBenchmarksAsync(CancellationToken cancellationToken = default)
    {
        var athlete = await _athleteService.GetCurrentUserAthleteAsync(cancellationToken);
        if (athlete == null)
        {
            return Array.Empty<AthleteBenchmarkDto>();
        }

        return await GetAthleteBenchmarksAsync(athlete.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<BenchmarkSummaryDto?> GetCurrentUserBenchmarkSummaryAsync(CancellationToken cancellationToken = default)
    {
        var athlete = await _athleteService.GetCurrentUserAthleteAsync(cancellationToken);
        if (athlete == null)
        {
            return null;
        }

        return await GetBenchmarkSummaryAsync(athlete.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(AthleteBenchmarkDto? Result, bool IsDuplicate, bool Unauthorized)> RecordCurrentUserBenchmarkAsync(
        RecordBenchmarkDto dto, CancellationToken cancellationToken = default)
    {
        var athlete = await _athleteService.GetCurrentUserAthleteAsync(cancellationToken);
        if (athlete == null)
        {
            return (null, false, true); // Unauthorized - no athlete profile
        }

        var (result, isDuplicate) = await RecordBenchmarkAsync(athlete.Id, dto, cancellationToken);
        return (result, isDuplicate, false);
    }

    /// <inheritdoc />
    public async Task<bool> ValidateOwnershipAsync(int athleteId, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return false;
        }

        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return false;
        }

        var athlete = await _database.Get<Athlete>()
            .Where(a => a.Id == athleteId && !a.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (athlete == null)
        {
            return false;
        }

        return athlete.UserId == userId.Value;
    }

    #endregion
}
