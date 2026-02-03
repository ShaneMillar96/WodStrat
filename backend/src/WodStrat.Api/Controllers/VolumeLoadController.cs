using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WodStrat.Api.Mappings;
using WodStrat.Api.ViewModels.VolumeLoad;
using WodStrat.Services.Interfaces;

namespace WodStrat.Api.Controllers;

/// <summary>
/// API endpoints for workout volume load calculations and analysis.
/// </summary>
[ApiController]
[Route("api")]
[Produces("application/json")]
[Tags("VolumeLoad")]
[Authorize]
public class VolumeLoadController : ControllerBase
{
    private readonly IVolumeLoadService _volumeLoadService;
    private readonly IBenchmarkService _benchmarkService;
    private readonly IWorkoutService _workoutService;

    public VolumeLoadController(
        IVolumeLoadService volumeLoadService,
        IBenchmarkService benchmarkService,
        IWorkoutService workoutService)
    {
        _volumeLoadService = volumeLoadService;
        _benchmarkService = benchmarkService;
        _workoutService = workoutService;
    }

    /// <summary>
    /// Get volume load analysis for a workout based on athlete's benchmarks and experience.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="workoutId">The workout's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Volume load analysis with per-movement breakdown and scaling tips.</returns>
    /// <response code="200">Returns volume load analysis.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Athlete or workout not found, or doesn't belong to user.</response>
    [HttpGet("athletes/{athleteId:int}/workouts/{workoutId:int}/volume-load")]
    [ProducesResponseType(typeof(WorkoutVolumeLoadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkoutVolumeLoadResponse>> GetWorkoutVolumeLoad(
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

        var result = await _volumeLoadService.CalculateWorkoutVolumeLoadAsync(athleteId, workoutId, ct);

        if (result is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Unable to calculate volume load. Ensure the workout has weighted movements."
            });
        }

        return Ok(result.ToResponse());
    }

    /// <summary>
    /// Calculate volume load analysis for a workout.
    /// </summary>
    /// <remarks>
    /// Alternative endpoint that accepts athlete and workout IDs in the request body.
    /// Useful for scenarios where the IDs need to be passed dynamically.
    /// </remarks>
    /// <param name="request">The volume load calculation request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Volume load analysis with per-movement breakdown and scaling tips.</returns>
    /// <response code="200">Returns volume load analysis.</response>
    /// <response code="400">Validation errors in request.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Athlete or workout not found, or doesn't belong to user.</response>
    [HttpPost("volume-load/calculate")]
    [ProducesResponseType(typeof(WorkoutVolumeLoadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkoutVolumeLoadResponse>> CalculateVolumeLoad(
        [FromBody] CalculateVolumeLoadRequest request,
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

        var result = await _volumeLoadService.CalculateWorkoutVolumeLoadAsync(request.AthleteId, request.WorkoutId, ct);

        if (result is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Unable to calculate volume load. Ensure the workout has weighted movements."
            });
        }

        return Ok(result.ToResponse());
    }
}
