using Microsoft.AspNetCore.Mvc;
using WodStrat.Api.Mappings;
using WodStrat.Api.ViewModels.Benchmarks;
using WodStrat.Services.Interfaces;

namespace WodStrat.Api.Controllers;

/// <summary>
/// API endpoints for athlete benchmark management.
/// </summary>
[ApiController]
[Route("api/athletes/{athleteId:int}/benchmarks")]
[Produces("application/json")]
[Tags("Athlete Benchmarks")]
public class AthleteBenchmarksController : ControllerBase
{
    private readonly IBenchmarkService _benchmarkService;
    private readonly IAthleteService _athleteService;

    public AthleteBenchmarksController(
        IBenchmarkService benchmarkService,
        IAthleteService athleteService)
    {
        _benchmarkService = benchmarkService;
        _athleteService = athleteService;
    }

    /// <summary>
    /// Get all benchmark results for an athlete.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of athlete benchmarks.</returns>
    /// <response code="200">Returns the list of athlete benchmarks.</response>
    /// <response code="404">Athlete not found.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AthleteBenchmarkResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<AthleteBenchmarkResponse>>> GetByAthlete(
        int athleteId,
        CancellationToken ct)
    {
        var athlete = await _athleteService.GetByIdAsync(athleteId, ct);
        if (athlete is null)
        {
            return NotFound();
        }

        var benchmarks = await _benchmarkService.GetAthleteBenchmarksAsync(athleteId, ct);
        var response = benchmarks.Select(b => b.ToResponse());
        return Ok(response);
    }

    /// <summary>
    /// Get benchmark summary for an athlete.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Benchmark completion summary.</returns>
    /// <response code="200">Returns the benchmark summary.</response>
    /// <response code="404">Athlete not found.</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(BenchmarkSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BenchmarkSummaryResponse>> GetSummary(
        int athleteId,
        CancellationToken ct)
    {
        var athlete = await _athleteService.GetByIdAsync(athleteId, ct);
        if (athlete is null)
        {
            return NotFound();
        }

        var summary = await _benchmarkService.GetBenchmarkSummaryAsync(athleteId, ct);
        return Ok(summary.ToResponse());
    }

    /// <summary>
    /// Get a specific benchmark result for an athlete.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="benchmarkId">The benchmark result's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The athlete benchmark.</returns>
    /// <response code="200">Returns the athlete benchmark.</response>
    /// <response code="404">Athlete or benchmark not found.</response>
    [HttpGet("{benchmarkId:int}")]
    [ProducesResponseType(typeof(AthleteBenchmarkResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AthleteBenchmarkResponse>> GetById(
        int athleteId,
        int benchmarkId,
        CancellationToken ct)
    {
        var athlete = await _athleteService.GetByIdAsync(athleteId, ct);
        if (athlete is null)
        {
            return NotFound();
        }

        var benchmark = await _benchmarkService.GetAthleteBenchmarkByIdAsync(athleteId, benchmarkId, ct);
        if (benchmark is null)
        {
            return NotFound();
        }

        return Ok(benchmark.ToResponse());
    }

    /// <summary>
    /// Record a new benchmark result for an athlete.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="request">The benchmark data to record.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created athlete benchmark.</returns>
    /// <response code="201">Benchmark recorded successfully.</response>
    /// <response code="400">Validation errors in request.</response>
    /// <response code="404">Athlete or benchmark definition not found.</response>
    /// <response code="409">Athlete already has a result for this benchmark.</response>
    [HttpPost]
    [ProducesResponseType(typeof(AthleteBenchmarkResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AthleteBenchmarkResponse>> Create(
        int athleteId,
        [FromBody] RecordBenchmarkRequest request,
        CancellationToken ct)
    {
        var athlete = await _athleteService.GetByIdAsync(athleteId, ct);
        if (athlete is null)
        {
            return NotFound();
        }

        var dto = request.ToDto();
        var (result, isDuplicate) = await _benchmarkService.RecordBenchmarkAsync(athleteId, dto, ct);

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
            return NotFound();
        }

        var response = result.ToResponse();
        return CreatedAtAction(
            nameof(GetById),
            new { athleteId, benchmarkId = response.Id },
            response);
    }

    /// <summary>
    /// Update an existing benchmark result.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="benchmarkId">The benchmark result's unique identifier.</param>
    /// <param name="request">The updated benchmark data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated athlete benchmark.</returns>
    /// <response code="200">Benchmark updated successfully.</response>
    /// <response code="400">Validation errors in request.</response>
    /// <response code="404">Athlete or benchmark not found.</response>
    [HttpPut("{benchmarkId:int}")]
    [ProducesResponseType(typeof(AthleteBenchmarkResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AthleteBenchmarkResponse>> Update(
        int athleteId,
        int benchmarkId,
        [FromBody] UpdateBenchmarkRequest request,
        CancellationToken ct)
    {
        var athlete = await _athleteService.GetByIdAsync(athleteId, ct);
        if (athlete is null)
        {
            return NotFound();
        }

        var dto = request.ToDto();
        var result = await _benchmarkService.UpdateBenchmarkAsync(athleteId, benchmarkId, dto, ct);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result.ToResponse());
    }

    /// <summary>
    /// Soft delete an athlete's benchmark result.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="benchmarkId">The benchmark result's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Benchmark deleted successfully.</response>
    /// <response code="404">Athlete or benchmark not found.</response>
    [HttpDelete("{benchmarkId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int athleteId,
        int benchmarkId,
        CancellationToken ct)
    {
        var athlete = await _athleteService.GetByIdAsync(athleteId, ct);
        if (athlete is null)
        {
            return NotFound();
        }

        var success = await _benchmarkService.DeleteBenchmarkAsync(athleteId, benchmarkId, ct);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}
