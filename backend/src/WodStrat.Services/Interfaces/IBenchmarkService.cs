using WodStrat.Services.Dtos;

namespace WodStrat.Services.Interfaces;

/// <summary>
/// Service interface for benchmark management operations.
/// Provides both benchmark definition queries and athlete benchmark CRUD operations.
/// </summary>
public interface IBenchmarkService
{
    #region Benchmark Definitions

    /// <summary>
    /// Retrieves all active benchmark definitions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of active benchmark definitions ordered by display order.</returns>
    Task<IReadOnlyList<BenchmarkDefinitionDto>> GetAllDefinitionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves active benchmark definitions filtered by category.
    /// </summary>
    /// <param name="category">The category name to filter by (case-insensitive).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of benchmark definitions in the specified category.</returns>
    Task<IReadOnlyList<BenchmarkDefinitionDto>> GetDefinitionsByCategoryAsync(string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a benchmark definition by its URL-friendly slug.
    /// </summary>
    /// <param name="slug">The benchmark definition's slug.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The benchmark definition if found and active; otherwise null.</returns>
    Task<BenchmarkDefinitionDto?> GetDefinitionBySlugAsync(string slug, CancellationToken cancellationToken = default);

    #endregion

    #region Athlete Benchmarks

    /// <summary>
    /// Retrieves all benchmark results for an athlete.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of athlete's benchmark results with definition details.</returns>
    Task<IReadOnlyList<AthleteBenchmarkDto>> GetAthleteBenchmarksAsync(int athleteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific benchmark for an athlete by benchmark ID.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="benchmarkId">The athlete benchmark's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The athlete benchmark if found and belongs to athlete; otherwise null.</returns>
    Task<AthleteBenchmarkDto?> GetAthleteBenchmarkByIdAsync(int athleteId, int benchmarkId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a benchmark summary for an athlete including completeness stats.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Summary with benchmark count and minimum requirement check.</returns>
    Task<BenchmarkSummaryDto> GetBenchmarkSummaryAsync(int athleteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a new benchmark for an athlete. Returns duplicate flag if already exists.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="dto">The benchmark data to record.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple containing the created benchmark (or null if duplicate) and duplicate flag.</returns>
    Task<(AthleteBenchmarkDto? Result, bool IsDuplicate)> RecordBenchmarkAsync(int athleteId, RecordBenchmarkDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing athlete benchmark value.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="benchmarkId">The athlete benchmark's unique identifier.</param>
    /// <param name="dto">The updated benchmark data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated athlete benchmark if found; otherwise null.</returns>
    Task<AthleteBenchmarkDto?> UpdateBenchmarkAsync(int athleteId, int benchmarkId, UpdateBenchmarkDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes an athlete benchmark.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="benchmarkId">The athlete benchmark's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the benchmark was found and deleted; otherwise false.</returns>
    Task<bool> DeleteBenchmarkAsync(int athleteId, int benchmarkId, CancellationToken cancellationToken = default);

    #endregion

    #region Current User Operations

    /// <summary>
    /// Gets benchmarks for the current user's athlete profile.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of benchmarks, or empty if user has no athlete profile.</returns>
    Task<IReadOnlyList<AthleteBenchmarkDto>> GetCurrentUserBenchmarksAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets benchmark summary for the current user's athlete profile.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Benchmark summary, or null if user has no athlete profile.</returns>
    Task<BenchmarkSummaryDto?> GetCurrentUserBenchmarkSummaryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a benchmark for the current user's athlete.
    /// </summary>
    /// <param name="dto">The benchmark data to record.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple containing the created benchmark (or null), duplicate flag, and unauthorized flag.</returns>
    Task<(AthleteBenchmarkDto? Result, bool IsDuplicate, bool Unauthorized)> RecordCurrentUserBenchmarkAsync(
        RecordBenchmarkDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the specified athlete belongs to the current user.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the athlete belongs to the current user; otherwise false.</returns>
    Task<bool> ValidateOwnershipAsync(int athleteId, CancellationToken cancellationToken = default);

    #endregion
}
