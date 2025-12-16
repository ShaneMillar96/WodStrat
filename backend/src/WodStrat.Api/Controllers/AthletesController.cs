using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WodStrat.Api.Mappings;
using WodStrat.Api.ViewModels.Athletes;
using WodStrat.Services.Interfaces;

namespace WodStrat.Api.Controllers;

/// <summary>
/// API endpoints for athlete profile management.
/// </summary>
[ApiController]
[Route("api/athletes")]
[Produces("application/json")]
[Tags("Athletes")]
[Authorize]
public class AthletesController : ControllerBase
{
    private readonly IAthleteService _athleteService;
    private readonly ICurrentUserService _currentUserService;

    public AthletesController(
        IAthleteService athleteService,
        ICurrentUserService currentUserService)
    {
        _athleteService = athleteService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get the authenticated user's athlete profile.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The athlete profile.</returns>
    /// <response code="200">Returns the athlete profile.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">User has no athlete profile.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(AthleteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AthleteResponse>> GetCurrentUserProfile(CancellationToken ct)
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

        return Ok(athlete.ToResponse());
    }

    /// <summary>
    /// Create athlete profile for the authenticated user.
    /// </summary>
    /// <param name="request">The athlete profile data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created athlete profile.</returns>
    /// <response code="201">Athlete profile created successfully.</response>
    /// <response code="400">Validation errors in request.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="409">User already has an athlete profile.</response>
    [HttpPost]
    [ProducesResponseType(typeof(AthleteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AthleteResponse>> Create(
        [FromBody] CreateAthleteRequest request,
        CancellationToken ct)
    {
        var dto = request.ToDto();
        var athlete = await _athleteService.CreateForCurrentUserAsync(dto, ct);

        if (athlete is null)
        {
            return Conflict(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                title = "Conflict",
                status = 409,
                detail = "User already has an athlete profile."
            });
        }

        var response = athlete.ToResponse();
        return CreatedAtAction(
            nameof(GetCurrentUserProfile),
            response);
    }

    /// <summary>
    /// Update the authenticated user's athlete profile.
    /// </summary>
    /// <param name="request">The updated athlete profile data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated athlete profile.</returns>
    /// <response code="200">Athlete profile updated successfully.</response>
    /// <response code="400">Validation errors in request.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">User has no athlete profile.</response>
    [HttpPut("me")]
    [ProducesResponseType(typeof(AthleteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AthleteResponse>> UpdateCurrentUserProfile(
        [FromBody] UpdateAthleteRequest request,
        CancellationToken ct)
    {
        // First, get the current user's athlete profile
        var currentAthlete = await _athleteService.GetCurrentUserAthleteAsync(ct);

        if (currentAthlete is null)
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
        var athlete = await _athleteService.UpdateAsync(currentAthlete.Id, dto, ct);

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

        return Ok(athlete.ToResponse());
    }

    /// <summary>
    /// Soft delete the authenticated user's athlete profile.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Athlete profile deleted successfully.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">User has no athlete profile.</response>
    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCurrentUserProfile(CancellationToken ct)
    {
        // First, get the current user's athlete profile
        var currentAthlete = await _athleteService.GetCurrentUserAthleteAsync(ct);

        if (currentAthlete is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "User has no athlete profile."
            });
        }

        var success = await _athleteService.DeleteAsync(currentAthlete.Id, ct);

        if (!success)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "User has no athlete profile."
            });
        }

        return NoContent();
    }
}
