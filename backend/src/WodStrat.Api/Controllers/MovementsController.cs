using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WodStrat.Api.Mappings;
using WodStrat.Api.ViewModels.Movements;
using WodStrat.Services.Interfaces;

namespace WodStrat.Api.Controllers;

/// <summary>
/// API endpoints for movement definitions (read-only reference data).
/// </summary>
[ApiController]
[Route("api/movements")]
[Produces("application/json")]
[Tags("Movements")]
[Authorize]
public class MovementsController : ControllerBase
{
    private readonly IMovementDefinitionService _movementDefinitionService;

    public MovementsController(IMovementDefinitionService movementDefinitionService)
    {
        _movementDefinitionService = movementDefinitionService;
    }

    /// <summary>
    /// Get all active movement definitions.
    /// </summary>
    /// <param name="category">Optional filter by category (Weightlifting, Gymnastics, Cardio, Strongman).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of movement definitions.</returns>
    /// <response code="200">Returns list of movement definitions.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MovementDefinitionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<MovementDefinitionResponse>>> GetAll(
        [FromQuery] string? category,
        CancellationToken ct)
    {
        var movements = string.IsNullOrWhiteSpace(category)
            ? await _movementDefinitionService.GetAllActiveMovementsAsync(ct)
            : await _movementDefinitionService.GetMovementsByCategoryAsync(category, ct);

        var response = movements.Select(m => m.ToResponse());
        return Ok(response);
    }

    /// <summary>
    /// Get a movement definition by canonical name.
    /// </summary>
    /// <param name="canonicalName">The canonical name (e.g., "pull_up", "thruster").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The movement definition.</returns>
    /// <response code="200">Returns the movement definition.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Movement not found.</response>
    [HttpGet("{canonicalName}")]
    [ProducesResponseType(typeof(MovementDefinitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovementDefinitionResponse>> GetByCanonicalName(
        string canonicalName,
        CancellationToken ct)
    {
        var movement = await _movementDefinitionService.GetMovementByCanonicalNameAsync(canonicalName, ct);

        if (movement is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Movement not found."
            });
        }

        return Ok(movement.ToResponse());
    }

    /// <summary>
    /// Search for a movement by alias or name.
    /// </summary>
    /// <param name="query">The search term (alias, display name, or canonical name).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The matched movement definition, or null if not found.</returns>
    /// <response code="200">Returns the matched movement definition (or null).</response>
    /// <response code="400">Query parameter is required.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(MovementDefinitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MovementDefinitionResponse?>> Search(
        [FromQuery] string query,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = 400,
                detail = "Query parameter is required."
            });
        }

        var movement = await _movementDefinitionService.FindMovementByAliasAsync(query, ct);

        if (movement is null)
        {
            return Ok(null);
        }

        return Ok(movement.ToResponse());
    }
}
