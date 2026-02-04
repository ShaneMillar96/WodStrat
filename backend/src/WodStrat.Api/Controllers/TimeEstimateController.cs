using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WodStrat.Api.Mappings;
using WodStrat.Api.ViewModels.TimeEstimate;
using WodStrat.Services.Interfaces;

namespace WodStrat.Api.Controllers;

/// <summary>
/// API endpoints for workout time estimation and EMOM feasibility analysis.
/// </summary>
[ApiController]
[Route("api")]
[Produces("application/json")]
[Tags("TimeEstimate")]
[Authorize]
public class TimeEstimateController : ControllerBase
{
    private readonly ITimeEstimateService _timeEstimateService;
    private readonly IBenchmarkService _benchmarkService;
    private readonly IWorkoutService _workoutService;

    public TimeEstimateController(
        ITimeEstimateService timeEstimateService,
        IBenchmarkService benchmarkService,
        IWorkoutService workoutService)
    {
        _timeEstimateService = timeEstimateService;
        _benchmarkService = benchmarkService;
        _workoutService = workoutService;
    }

    /// <summary>
    /// Get time estimate for a workout based on athlete's benchmarks and performance data.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="workoutId">The workout's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Time estimate with confidence level, factors summary, and rest recommendations.</returns>
    /// <response code="200">Returns time estimate.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Athlete or workout not found, or doesn't belong to user.</response>
    [HttpGet("athletes/{athleteId:int}/workouts/{workoutId:int}/time-estimate")]
    [ProducesResponseType(typeof(TimeEstimateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TimeEstimateResponse>> GetTimeEstimate(
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

        var result = await _timeEstimateService.EstimateWorkoutTimeAsync(athleteId, workoutId, ct);

        if (result is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Unable to calculate time estimate. Ensure athlete has sufficient benchmark data."
            });
        }

        return Ok(result.ToResponse());
    }

    /// <summary>
    /// Calculate time estimate for a workout.
    /// </summary>
    /// <remarks>
    /// Alternative endpoint that accepts athlete and workout IDs in the request body.
    /// Useful for scenarios where the IDs need to be passed dynamically.
    /// </remarks>
    /// <param name="request">The time estimate calculation request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Time estimate with confidence level, factors summary, and rest recommendations.</returns>
    /// <response code="200">Returns time estimate.</response>
    /// <response code="400">Validation errors in request.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Athlete or workout not found, or doesn't belong to user.</response>
    [HttpPost("time-estimate/calculate")]
    [ProducesResponseType(typeof(TimeEstimateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TimeEstimateResponse>> CalculateTimeEstimate(
        [FromBody] CalculateTimeEstimateRequest request,
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

        var result = await _timeEstimateService.EstimateWorkoutTimeAsync(request.AthleteId, request.WorkoutId, ct);

        if (result is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Unable to calculate time estimate. Ensure athlete has sufficient benchmark data."
            });
        }

        return Ok(result.ToResponse());
    }

    /// <summary>
    /// Get EMOM feasibility analysis for a workout.
    /// </summary>
    /// <remarks>
    /// Analyzes each minute of an EMOM workout to determine if the prescribed work
    /// can be completed within the time constraint, with buffer analysis and recommendations.
    /// </remarks>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="workoutId">The workout's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>EMOM feasibility analysis with per-minute breakdown.</returns>
    /// <response code="200">Returns EMOM feasibility analysis.</response>
    /// <response code="400">Workout is not an EMOM type.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Athlete or workout not found, or doesn't belong to user.</response>
    [HttpGet("athletes/{athleteId:int}/workouts/{workoutId:int}/emom-feasibility")]
    [ProducesResponseType(typeof(EmomFeasibilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmomFeasibilityResponse>> GetEmomFeasibility(
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

        // Get time estimate first to check workout type and get EMOM feasibility
        var timeEstimate = await _timeEstimateService.EstimateWorkoutTimeAsync(athleteId, workoutId, ct);

        if (timeEstimate is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Unable to calculate EMOM feasibility. Ensure the workout exists and athlete has sufficient benchmark data."
            });
        }

        // Check if workout is an EMOM
        if (!timeEstimate.WorkoutType.Equals("Emom", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = 400,
                detail = $"EMOM feasibility analysis is only available for EMOM workouts. This workout is of type '{timeEstimate.WorkoutType}'."
            });
        }

        // The EMOM feasibility data is included in the time estimate result
        if (timeEstimate.EmomFeasibility is null || !timeEstimate.EmomFeasibility.Any())
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Unable to calculate EMOM feasibility. Ensure the workout has movements defined."
            });
        }

        var response = timeEstimate.EmomFeasibility.ToEmomFeasibilityResponse(
            timeEstimate.WorkoutId,
            timeEstimate.WorkoutName);

        return Ok(response);
    }
}
