using Microsoft.EntityFrameworkCore;
using WodStrat.Dal.Interfaces;
using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;
using WodStrat.Services.Extensions;
using WodStrat.Services.Interfaces;
using WodStrat.Services.Utilities;

namespace WodStrat.Services.Services;

/// <summary>
/// Service implementation for building unified movement context data
/// that is shared across all strategy services.
/// This eliminates duplicate database queries and ensures consistent benchmark data.
/// </summary>
public class MovementContextService : IMovementContextService
{
    private readonly IWodStratDatabase _database;
    private readonly IAthleteService _athleteService;

    /// <summary>
    /// Initializes a new instance of the MovementContextService.
    /// </summary>
    /// <param name="database">Database access interface.</param>
    /// <param name="athleteService">Athlete service for current user operations.</param>
    public MovementContextService(
        IWodStratDatabase database,
        IAthleteService athleteService)
    {
        _database = database;
        _athleteService = athleteService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MovementContextDto>> BuildMovementContextsAsync(
        int athleteId,
        int workoutId,
        CancellationToken cancellationToken = default)
    {
        // Load workout with movements
        var workout = await _database.Get<Workout>()
            .Include(w => w.Movements)
                .ThenInclude(m => m.MovementDefinition)
            .Where(w => w.Id == workoutId && !w.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (workout == null)
        {
            return Array.Empty<MovementContextDto>();
        }

        // Get all movement definition IDs from the workout
        var movementDefinitions = workout.Movements
            .Select(m => m.MovementDefinition)
            .DistinctBy(m => m.Id)
            .ToList();

        // Build contexts for all movements
        var contexts = new List<MovementContextDto>();
        foreach (var movementDef in movementDefinitions)
        {
            var context = await BuildMovementContextAsync(athleteId, movementDef, cancellationToken);
            contexts.Add(context);
        }

        return contexts;
    }

    /// <inheritdoc />
    public async Task<WorkoutContextDto?> BuildWorkoutContextAsync(
        int athleteId,
        int workoutId,
        CancellationToken cancellationToken = default)
    {
        // Load workout with movements
        var workout = await _database.Get<Workout>()
            .Include(w => w.Movements)
                .ThenInclude(m => m.MovementDefinition)
            .Where(w => w.Id == workoutId && !w.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (workout == null)
        {
            return null;
        }

        // Build movement contexts
        var movementContexts = await BuildMovementContextsForWorkoutAsync(
            athleteId,
            workout,
            cancellationToken);

        return workout.ToWorkoutContextDto(movementContexts);
    }

    /// <inheritdoc />
    public async Task<MovementContextDto> BuildMovementContextAsync(
        int athleteId,
        MovementDefinition movementDefinition,
        CancellationToken cancellationToken = default)
    {
        // Find the best benchmark mapping for this movement
        var benchmarkMapping = await FindBestBenchmarkMappingAsync(
            athleteId,
            movementDefinition.Id,
            cancellationToken);

        // Initialize default values
        var benchmarkName = "None";
        decimal? percentile = null;
        var hasPopulationData = false;
        var hasAthleteBenchmark = false;

        if (benchmarkMapping != null)
        {
            benchmarkName = benchmarkMapping.BenchmarkDefinition.Name;

            // Get athlete's benchmark for this definition
            var athleteBenchmark = await _database.Get<AthleteBenchmark>()
                .Where(ab => ab.AthleteId == athleteId
                    && ab.BenchmarkDefinitionId == benchmarkMapping.BenchmarkDefinitionId
                    && !ab.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            // Get population percentile data
            var populationData = await _database.Get<PopulationBenchmarkPercentile>()
                .Where(p => p.BenchmarkDefinitionId == benchmarkMapping.BenchmarkDefinitionId)
                .FirstOrDefaultAsync(cancellationToken);

            hasPopulationData = populationData != null;
            hasAthleteBenchmark = athleteBenchmark != null;

            if (hasPopulationData && hasAthleteBenchmark)
            {
                percentile = PercentileCalculator.CalculatePercentile(
                    athleteBenchmark!.Value,
                    populationData!,
                    benchmarkMapping.BenchmarkDefinition.MetricType);
            }
        }

        return movementDefinition.ToMovementContextDto(
            benchmarkName,
            percentile,
            hasPopulationData,
            hasAthleteBenchmark);
    }

    /// <inheritdoc />
    public async Task<WorkoutContextDto?> BuildCurrentUserWorkoutContextAsync(
        int workoutId,
        CancellationToken cancellationToken = default)
    {
        var athlete = await _athleteService.GetCurrentUserAthleteAsync(cancellationToken);
        if (athlete == null)
        {
            return null;
        }

        return await BuildWorkoutContextAsync(athlete.Id, workoutId, cancellationToken);
    }

    #region Private Helper Methods

    /// <summary>
    /// Builds movement contexts for all movements in a pre-loaded workout.
    /// </summary>
    private async Task<IReadOnlyList<MovementContextDto>> BuildMovementContextsForWorkoutAsync(
        int athleteId,
        Workout workout,
        CancellationToken cancellationToken)
    {
        // Get unique movement definitions from the workout
        var movementDefinitions = workout.Movements
            .Select(m => m.MovementDefinition)
            .DistinctBy(m => m.Id)
            .ToList();

        // Get all benchmark definition IDs that the athlete has recorded (for optimization)
        var athleteBenchmarkDefIds = await _database.Get<AthleteBenchmark>()
            .Where(ab => ab.AthleteId == athleteId && !ab.IsDeleted)
            .Select(ab => ab.BenchmarkDefinitionId)
            .ToListAsync(cancellationToken);

        var contexts = new List<MovementContextDto>();

        foreach (var movementDef in movementDefinitions)
        {
            var context = await BuildMovementContextWithPrefetchedDataAsync(
                athleteId,
                movementDef,
                athleteBenchmarkDefIds,
                cancellationToken);
            contexts.Add(context);
        }

        return contexts;
    }

    /// <summary>
    /// Builds movement context with pre-fetched athlete benchmark IDs for better performance.
    /// </summary>
    private async Task<MovementContextDto> BuildMovementContextWithPrefetchedDataAsync(
        int athleteId,
        MovementDefinition movementDefinition,
        IReadOnlyList<int> athleteBenchmarkDefIds,
        CancellationToken cancellationToken)
    {
        // Find the best benchmark mapping for this movement
        var benchmarkMapping = await FindBestBenchmarkMappingWithPrefetchAsync(
            movementDefinition.Id,
            athleteBenchmarkDefIds,
            cancellationToken);

        // Initialize default values
        var benchmarkName = "None";
        decimal? percentile = null;
        var hasPopulationData = false;
        var hasAthleteBenchmark = false;

        if (benchmarkMapping != null)
        {
            benchmarkName = benchmarkMapping.BenchmarkDefinition.Name;

            // Check if athlete has this benchmark
            hasAthleteBenchmark = athleteBenchmarkDefIds.Contains(benchmarkMapping.BenchmarkDefinitionId);

            // Get population percentile data
            var populationData = await _database.Get<PopulationBenchmarkPercentile>()
                .Where(p => p.BenchmarkDefinitionId == benchmarkMapping.BenchmarkDefinitionId)
                .FirstOrDefaultAsync(cancellationToken);

            hasPopulationData = populationData != null;

            if (hasPopulationData && hasAthleteBenchmark)
            {
                // Get athlete's benchmark value
                var athleteBenchmark = await _database.Get<AthleteBenchmark>()
                    .Where(ab => ab.AthleteId == athleteId
                        && ab.BenchmarkDefinitionId == benchmarkMapping.BenchmarkDefinitionId
                        && !ab.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (athleteBenchmark != null)
                {
                    percentile = PercentileCalculator.CalculatePercentile(
                        athleteBenchmark.Value,
                        populationData!,
                        benchmarkMapping.BenchmarkDefinition.MetricType);
                }
            }
        }

        return movementDefinition.ToMovementContextDto(
            benchmarkName,
            percentile,
            hasPopulationData,
            hasAthleteBenchmark);
    }

    /// <summary>
    /// Finds the best benchmark mapping for a movement, prioritizing mappings where the athlete has recorded a benchmark.
    /// </summary>
    private async Task<BenchmarkMovementMapping?> FindBestBenchmarkMappingAsync(
        int athleteId,
        int movementDefinitionId,
        CancellationToken cancellationToken)
    {
        // Get all benchmark mappings for this movement
        var allMappings = await _database.Get<BenchmarkMovementMapping>()
            .Include(m => m.BenchmarkDefinition)
            .Where(m => m.MovementDefinitionId == movementDefinitionId)
            .OrderByDescending(m => m.RelevanceFactor)
            .ToListAsync(cancellationToken);

        if (allMappings.Count == 0)
        {
            return null;
        }

        // Get all benchmark definition IDs that the athlete has recorded
        var athleteBenchmarkDefIds = await _database.Get<AthleteBenchmark>()
            .Where(ab => ab.AthleteId == athleteId && !ab.IsDeleted)
            .Select(ab => ab.BenchmarkDefinitionId)
            .ToListAsync(cancellationToken);

        // First, try to find a mapping where the athlete has a benchmark (prioritize by relevance)
        var mappingWithAthleteBenchmark = allMappings
            .Where(m => athleteBenchmarkDefIds.Contains(m.BenchmarkDefinitionId))
            .OrderByDescending(m => m.RelevanceFactor)
            .FirstOrDefault();

        if (mappingWithAthleteBenchmark != null)
        {
            return mappingWithAthleteBenchmark;
        }

        // Fall back to the first mapping by relevance
        return allMappings[0];
    }

    /// <summary>
    /// Finds the best benchmark mapping with pre-fetched athlete benchmark IDs for better performance.
    /// </summary>
    private async Task<BenchmarkMovementMapping?> FindBestBenchmarkMappingWithPrefetchAsync(
        int movementDefinitionId,
        IReadOnlyList<int> athleteBenchmarkDefIds,
        CancellationToken cancellationToken)
    {
        // Get all benchmark mappings for this movement
        var allMappings = await _database.Get<BenchmarkMovementMapping>()
            .Include(m => m.BenchmarkDefinition)
            .Where(m => m.MovementDefinitionId == movementDefinitionId)
            .OrderByDescending(m => m.RelevanceFactor)
            .ToListAsync(cancellationToken);

        if (allMappings.Count == 0)
        {
            return null;
        }

        // First, try to find a mapping where the athlete has a benchmark (prioritize by relevance)
        var mappingWithAthleteBenchmark = allMappings
            .Where(m => athleteBenchmarkDefIds.Contains(m.BenchmarkDefinitionId))
            .OrderByDescending(m => m.RelevanceFactor)
            .FirstOrDefault();

        if (mappingWithAthleteBenchmark != null)
        {
            return mappingWithAthleteBenchmark;
        }

        // Fall back to the first mapping by relevance
        return allMappings[0];
    }

    #endregion
}
