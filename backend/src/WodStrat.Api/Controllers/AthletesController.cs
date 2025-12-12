using Microsoft.AspNetCore.Mvc;
using WodStrat.Api.Mappings;
using WodStrat.Api.ViewModels.Athletes;
using WodStrat.Services.Interfaces;

namespace WodStrat.Api.Controllers;

/// <summary>
/// API endpoints for athlete profile management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AthletesController : ControllerBase
{
    private readonly IAthleteService _athleteService;

    public AthletesController(IAthleteService athleteService)
    {
        _athleteService = athleteService;
    }

    /// <summary>
    /// Get athlete profile by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the athlete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The athlete profile.</returns>
    /// <response code="200">Returns the athlete profile.</response>
    /// <response code="404">Athlete not found or has been deleted.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AthleteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AthleteResponse>> GetById(Guid id, CancellationToken ct)
    {
        var athlete = await _athleteService.GetByIdAsync(id, ct);

        if (athlete is null)
        {
            return NotFound();
        }

        return Ok(athlete.ToResponse());
    }

    /// <summary>
    /// Get current user's athlete profile.
    /// </summary>
    /// <returns>501 Not Implemented until authentication is added.</returns>
    /// <response code="501">Authentication not yet implemented.</response>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult GetCurrentUserProfile()
    {
        return StatusCode(StatusCodes.Status501NotImplemented, new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.6.2",
            title = "Not Implemented",
            status = 501,
            detail = "The /api/athletes/me endpoint requires authentication which is not yet implemented."
        });
    }

    /// <summary>
    /// Create a new athlete profile.
    /// </summary>
    /// <param name="request">The athlete profile data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created athlete profile.</returns>
    /// <response code="201">Athlete profile created successfully.</response>
    /// <response code="400">Validation errors in request.</response>
    [HttpPost]
    [ProducesResponseType(typeof(AthleteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AthleteResponse>> Create(
        [FromBody] CreateAthleteRequest request,
        CancellationToken ct)
    {
        var dto = request.ToDto();
        var athlete = await _athleteService.CreateAsync(dto, ct);
        var response = athlete.ToResponse();

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    /// <summary>
    /// Update an existing athlete profile.
    /// </summary>
    /// <param name="id">The unique identifier of the athlete.</param>
    /// <param name="request">The updated athlete profile data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated athlete profile.</returns>
    /// <response code="200">Athlete profile updated successfully.</response>
    /// <response code="400">Validation errors in request.</response>
    /// <response code="404">Athlete not found or has been deleted.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AthleteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AthleteResponse>> Update(
        Guid id,
        [FromBody] UpdateAthleteRequest request,
        CancellationToken ct)
    {
        var dto = request.ToDto();
        var athlete = await _athleteService.UpdateAsync(id, dto, ct);

        if (athlete is null)
        {
            return NotFound();
        }

        return Ok(athlete.ToResponse());
    }

    /// <summary>
    /// Soft delete an athlete profile.
    /// </summary>
    /// <param name="id">The unique identifier of the athlete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Athlete profile deleted successfully.</response>
    /// <response code="404">Athlete not found or already deleted.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var success = await _athleteService.DeleteAsync(id, ct);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}
