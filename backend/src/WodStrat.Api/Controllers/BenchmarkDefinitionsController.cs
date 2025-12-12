using Microsoft.AspNetCore.Mvc;
using WodStrat.Api.Mappings;
using WodStrat.Api.ViewModels.Benchmarks;
using WodStrat.Services.Interfaces;

namespace WodStrat.Api.Controllers;

/// <summary>
/// API endpoints for benchmark definitions (read-only).
/// </summary>
[ApiController]
[Route("api/benchmarks/definitions")]
[Produces("application/json")]
[Tags("Benchmark Definitions")]
public class BenchmarkDefinitionsController : ControllerBase
{
    private readonly IBenchmarkService _benchmarkService;

    public BenchmarkDefinitionsController(IBenchmarkService benchmarkService)
    {
        _benchmarkService = benchmarkService;
    }

    /// <summary>
    /// Get all active benchmark definitions.
    /// </summary>
    /// <param name="category">Optional filter by category (Cardio, Strength, Gymnastics, HeroWod).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of benchmark definitions.</returns>
    /// <response code="200">Returns the list of benchmark definitions.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BenchmarkDefinitionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BenchmarkDefinitionResponse>>> GetAll(
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
    /// <response code="404">Benchmark definition not found.</response>
    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(BenchmarkDefinitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BenchmarkDefinitionResponse>> GetBySlug(
        string slug,
        CancellationToken ct)
    {
        var definition = await _benchmarkService.GetDefinitionBySlugAsync(slug, ct);

        if (definition is null)
        {
            return NotFound();
        }

        return Ok(definition.ToResponse());
    }
}
