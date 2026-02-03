using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WodStrat.Api.Mappings;
using WodStrat.Api.ViewModels.Pacing;
using WodStrat.Services.Interfaces;

namespace WodStrat.Api.Controllers;

/// <summary>
/// API endpoints for workout pacing recommendations.
/// </summary>
[ApiController]
[Route("api")]
[Produces("application/json")]
[Tags("Pacing")]
[Authorize]
public class PacingController : ControllerBase
{
    private readonly IPacingService _pacingService;
    private readonly IBenchmarkService _benchmarkService;
    private readonly IWorkoutService _workoutService;

    public PacingController(
        IPacingService pacingService,
        IBenchmarkService benchmarkService,
        IWorkoutService workoutService)
    {
        _pacingService = pacingService;
        _benchmarkService = benchmarkService;
        _workoutService = workoutService;
    }

    /// <summary>
    /// Get pacing recommendations for a workout based on athlete's benchmarks.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="workoutId">The workout's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Pacing recommendations for each movement in the workout.</returns>
    /// <response code="200">Returns pacing recommendations.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Athlete or workout not found, or doesn't belong to user.</response>
    [HttpGet("athletes/{athleteId:int}/workouts/{workoutId:int}/pacing")]
    [ProducesResponseType(typeof(WorkoutPacingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkoutPacingResponse>> GetWorkoutPacing(
        int athleteId,
        int workoutId,
        CancellationToken ct)
    {
        // Verify athlete ownership
        var athleteOwned = await _benchmarkService.ValidateOwnershipAsync(athleteId, ct);
        if (!athleteOwned)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Athlete not found or doesn't belong to user."
            });
        }

        // Verify workout ownership
        var workoutOwned = await _workoutService.ValidateOwnershipAsync(workoutId, ct);
        if (!workoutOwned)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Workout not found or doesn't belong to user."
            });
        }

        var result = await _pacingService.CalculateWorkoutPacingAsync(athleteId, workoutId, ct);

        if (result is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Unable to calculate pacing. Ensure athlete has sufficient benchmark data."
            });
        }

        return Ok(result.ToResponse());
    }

    /// <summary>
    /// Calculate pacing recommendations for a workout.
    /// </summary>
    /// <remarks>
    /// Alternative endpoint that accepts athlete and workout IDs in the request body.
    /// Useful for scenarios where the IDs need to be passed dynamically.
    /// </remarks>
    /// <param name="request">The pacing calculation request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Pacing recommendations for each movement in the workout.</returns>
    /// <response code="200">Returns pacing recommendations.</response>
    /// <response code="400">Validation errors in request.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Athlete or workout not found, or doesn't belong to user.</response>
    [HttpPost("pacing/calculate")]
    [ProducesResponseType(typeof(WorkoutPacingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkoutPacingResponse>> CalculatePacing(
        [FromBody] CalculatePacingRequest request,
        CancellationToken ct)
    {
        // Verify athlete ownership
        var athleteOwned = await _benchmarkService.ValidateOwnershipAsync(request.AthleteId, ct);
        if (!athleteOwned)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Athlete not found or doesn't belong to user."
            });
        }

        // Verify workout ownership
        var workoutOwned = await _workoutService.ValidateOwnershipAsync(request.WorkoutId, ct);
        if (!workoutOwned)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Workout not found or doesn't belong to user."
            });
        }

        var result = await _pacingService.CalculateWorkoutPacingAsync(request.AthleteId, request.WorkoutId, ct);

        if (result is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Unable to calculate pacing. Ensure athlete has sufficient benchmark data."
            });
        }

        return Ok(result.ToResponse());
    }

    /// <summary>
    /// Get pacing recommendation for a single movement.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="movementDefinitionId">The movement definition's unique identifier.</param>
    /// <param name="repCount">The number of repetitions to calculate pacing for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Pacing recommendation for the movement.</returns>
    /// <response code="200">Returns pacing recommendation.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Athlete or movement not found, or doesn't belong to user.</response>
    [HttpGet("athletes/{athleteId:int}/movements/{movementDefinitionId:int}/pacing")]
    [ProducesResponseType(typeof(MovementPacingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovementPacingResponse>> GetMovementPacing(
        int athleteId,
        int movementDefinitionId,
        [FromQuery] int repCount,
        CancellationToken ct)
    {
        // Verify athlete ownership
        var athleteOwned = await _benchmarkService.ValidateOwnershipAsync(athleteId, ct);
        if (!athleteOwned)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Athlete not found or doesn't belong to user."
            });
        }

        var result = await _pacingService.CalculateMovementPacingAsync(athleteId, movementDefinitionId, repCount, ct);

        if (result is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Movement not found or insufficient benchmark data for pacing calculation."
            });
        }

        return Ok(result.ToResponse());
    }
}
