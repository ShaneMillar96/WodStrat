using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WodStrat.Api.Mappings;
using WodStrat.Api.ViewModels.Benchmarks;
using WodStrat.Services.Interfaces;

namespace WodStrat.Api.Controllers;

/// <summary>
/// API endpoints for benchmark management.
/// </summary>
[ApiController]
[Route("api/benchmarks")]
[Produces("application/json")]
[Tags("Benchmarks")]
[Authorize]
public class BenchmarksController : ControllerBase
{
    private readonly IBenchmarkService _benchmarkService;
    private readonly IAthleteService _athleteService;
    private readonly ICurrentUserService _currentUserService;

    public BenchmarksController(
        IBenchmarkService benchmarkService,
        IAthleteService athleteService,
        ICurrentUserService currentUserService)
    {
        _benchmarkService = benchmarkService;
        _athleteService = athleteService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get all benchmark results for the authenticated user's athlete.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of athlete benchmarks.</returns>
    /// <response code="200">Returns the list of athlete benchmarks.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">User has no athlete profile.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AthleteBenchmarkResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<AthleteBenchmarkResponse>>> GetAll(CancellationToken ct)
    {
        var athlete = await _athleteService.GetCurrentUserAthleteAsync(ct);
        if (athlete is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "User has no athlete profile."
            });
        }

        var benchmarks = await _benchmarkService.GetCurrentUserBenchmarksAsync(ct);
        var response = benchmarks.Select(b => b.ToResponse());
        return Ok(response);
    }

    /// <summary>
    /// Get benchmark summary for the authenticated user's athlete.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Benchmark completion summary.</returns>
    /// <response code="200">Returns the benchmark summary.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">User has no athlete profile.</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(BenchmarkSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BenchmarkSummaryResponse>> GetSummary(CancellationToken ct)
    {
        var summary = await _benchmarkService.GetCurrentUserBenchmarkSummaryAsync(ct);
        if (summary is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "User has no athlete profile."
            });
        }

        return Ok(summary.ToResponse());
    }

    /// <summary>
    /// Get all active benchmark definitions.
    /// </summary>
    /// <param name="category">Optional filter by category (Cardio, Strength, Gymnastics, HeroWod).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of benchmark definitions.</returns>
    /// <response code="200">Returns the list of benchmark definitions.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpGet("definitions")]
    [ProducesResponseType(typeof(IEnumerable<BenchmarkDefinitionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<BenchmarkDefinitionResponse>>> GetDefinitions(
        [FromQuery] string? category,
        CancellationToken ct)
    {
        var definitions = string.IsNullOrWhiteSpace(category)
            ? await _benchmarkService.GetAllDefinitionsAsync(ct)
            : await _benchmarkService.GetDefinitionsByCategoryAsync(category, ct);

        var response = definitions.Select(d => d.ToResponse());
        return Ok(response);
    }

    /// <summary>
    /// Get a benchmark definition by slug.
    /// </summary>
    /// <param name="slug">The URL-friendly identifier of the benchmark.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The benchmark definition.</returns>
    /// <response code="200">Returns the benchmark definition.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Benchmark definition not found.</response>
    [HttpGet("definitions/{slug}")]
    [ProducesResponseType(typeof(BenchmarkDefinitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BenchmarkDefinitionResponse>> GetDefinitionBySlug(
        string slug,
        CancellationToken ct)
    {
        var definition = await _benchmarkService.GetDefinitionBySlugAsync(slug, ct);

        if (definition is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Benchmark definition not found."
            });
        }

        return Ok(definition.ToResponse());
    }

    /// <summary>
    /// Record a new benchmark result for the authenticated user's athlete.
    /// </summary>
    /// <param name="request">The benchmark data to record.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created athlete benchmark.</returns>
    /// <response code="201">Benchmark recorded successfully.</response>
    /// <response code="400">Validation errors in request.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">User has no athlete profile or benchmark definition not found.</response>
    /// <response code="409">Athlete already has a result for this benchmark.</response>
    [HttpPost]
    [ProducesResponseType(typeof(AthleteBenchmarkResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AthleteBenchmarkResponse>> Create(
        [FromBody] RecordBenchmarkRequest request,
        CancellationToken ct)
    {
        var dto = request.ToDto();
        var (result, isDuplicate, unauthorized) = await _benchmarkService.RecordCurrentUserBenchmarkAsync(dto, ct);

        if (unauthorized)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "User has no athlete profile."
            });
        }

        if (isDuplicate)
        {
            return Conflict(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                title = "Conflict",
                status = 409,
                detail = "Athlete already has a recorded benchmark for this definition. Use PUT to update the existing result."
            });
        }

        if (result is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Benchmark definition not found."
            });
        }

        var response = result.ToResponse();
        return CreatedAtAction(
            nameof(GetById),
            new { benchmarkId = response.Id },
            response);
    }

    /// <summary>
    /// Get a specific benchmark result (ownership verified).
    /// </summary>
    /// <param name="benchmarkId">The benchmark result's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The athlete benchmark.</returns>
    /// <response code="200">Returns the athlete benchmark.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Benchmark not found or doesn't belong to user.</response>
    [HttpGet("{benchmarkId:int}")]
    [ProducesResponseType(typeof(AthleteBenchmarkResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AthleteBenchmarkResponse>> GetById(
        int benchmarkId,
        CancellationToken ct)
    {
        var athlete = await _athleteService.GetCurrentUserAthleteAsync(ct);
        if (athlete is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "User has no athlete profile."
            });
        }

        var benchmark = await _benchmarkService.GetAthleteBenchmarkByIdAsync(athlete.Id, benchmarkId, ct);
        if (benchmark is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Benchmark not found or doesn't belong to user."
            });
        }

        return Ok(benchmark.ToResponse());
    }

    /// <summary>
    /// Update a benchmark result (ownership verified).
    /// </summary>
    /// <param name="benchmarkId">The benchmark result's unique identifier.</param>
    /// <param name="request">The updated benchmark data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated athlete benchmark.</returns>
    /// <response code="200">Benchmark updated successfully.</response>
    /// <response code="400">Validation errors in request.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Benchmark not found or doesn't belong to user.</response>
    [HttpPut("{benchmarkId:int}")]
    [ProducesResponseType(typeof(AthleteBenchmarkResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AthleteBenchmarkResponse>> Update(
        int benchmarkId,
        [FromBody] UpdateBenchmarkRequest request,
        CancellationToken ct)
    {
        var athlete = await _athleteService.GetCurrentUserAthleteAsync(ct);
        if (athlete is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "User has no athlete profile."
            });
        }

        var dto = request.ToDto();
        var result = await _benchmarkService.UpdateBenchmarkAsync(athlete.Id, benchmarkId, dto, ct);

        if (result is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Benchmark not found or doesn't belong to user."
            });
        }

        return Ok(result.ToResponse());
    }

    /// <summary>
    /// Soft delete a benchmark result (ownership verified).
    /// </summary>
    /// <param name="benchmarkId">The benchmark result's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Benchmark deleted successfully.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Benchmark not found or doesn't belong to user.</response>
    [HttpDelete("{benchmarkId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int benchmarkId,
        CancellationToken ct)
    {
        var athlete = await _athleteService.GetCurrentUserAthleteAsync(ct);
        if (athlete is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "User has no athlete profile."
            });
        }

        var success = await _benchmarkService.DeleteBenchmarkAsync(athlete.Id, benchmarkId, ct);
        if (!success)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Benchmark not found or doesn't belong to user."
            });
        }

        return NoContent();
    }
}
