using Microsoft.AspNetCore.Mvc;
using WodStrat.Api.Mappings;
using WodStrat.Api.ViewModels.Movements;
using WodStrat.Services.Interfaces;

namespace WodStrat.Api.Controllers;

/// <summary>
/// API endpoints for movement definitions (read-only reference data).
/// Provides access to the canonical movement library and alias lookups.
/// </summary>
[ApiController]
[Route("api/movements")]
[Produces("application/json")]
[Tags("Movements")]
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
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MovementDefinitionResponse>), StatusCodes.Status200OK)]
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
    /// <response code="404">Movement not found.</response>
    [HttpGet("{canonicalName}")]
    [ProducesResponseType(typeof(MovementDefinitionResponse), StatusCodes.Status200OK)]
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
    /// Lookup a movement by its alias.
    /// </summary>
    /// <remarks>
    /// Performs a direct alias-to-movement lookup. Common aliases include:
    /// - "T2B" -> toes_to_bar
    /// - "C2B" -> chest_to_bar
    /// - "HSPU" -> handstand_push_up
    /// - "DU" -> double_under
    ///
    /// This endpoint returns 404 if no match is found, unlike the search endpoint which returns null.
    /// </remarks>
    /// <param name="alias">The alias to lookup (e.g., "T2B", "C2B", "HSPU").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The matched movement definition.</returns>
    /// <response code="200">Returns the matched movement definition.</response>
    /// <response code="400">Alias parameter is required.</response>
    /// <response code="404">No movement found for the given alias.</response>
    [HttpGet("lookup")]
    [ProducesResponseType(typeof(MovementDefinitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovementDefinitionResponse>> Lookup(
        [FromQuery] string alias,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            return BadRequest(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = 400,
                detail = "Alias parameter is required."
            });
        }

        var movement = await _movementDefinitionService.FindMovementByAliasAsync(alias, ct);

        if (movement is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = $"No movement found for alias '{alias}'."
            });
        }

        return Ok(movement.ToResponse());
    }

    /// <summary>
    /// Search for a movement by alias or name.
    /// </summary>
    /// <remarks>
    /// Searches across aliases, display names, and canonical names.
    /// Returns null if no match is found (unlike the lookup endpoint which returns 404).
    /// </remarks>
    /// <param name="q">The search term (alias, display name, or canonical name).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The matched movement definition, or null if not found.</returns>
    /// <response code="200">Returns the matched movement definition (or null).</response>
    /// <response code="400">Query parameter is required.</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(MovementDefinitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MovementDefinitionResponse?>> Search(
        [FromQuery] string q,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = 400,
                detail = "Query parameter is required."
            });
        }

        var movement = await _movementDefinitionService.FindMovementByAliasAsync(q, ct);

        if (movement is null)
        {
            return Ok(null);
        }

        return Ok(movement.ToResponse());
    }
}
