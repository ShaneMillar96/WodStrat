using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WodStrat.Api.Mappings;
using WodStrat.Api.ViewModels.Strategy;
using WodStrat.Services.Interfaces;

namespace WodStrat.Api.Controllers;

/// <summary>
/// API endpoints for unified workout strategy analysis.
/// </summary>
[ApiController]
[Route("api")]
[Produces("application/json")]
[Tags("Strategy")]
[Authorize]
public class StrategyController : ControllerBase
{
    private readonly IUnifiedStrategyService _unifiedStrategyService;
    private readonly IBenchmarkService _benchmarkService;
    private readonly IWorkoutService _workoutService;

    /// <summary>
    /// Initializes a new instance of the <see cref="StrategyController"/> class.
    /// </summary>
    /// <param name="unifiedStrategyService">The unified strategy service.</param>
    /// <param name="benchmarkService">The benchmark service for ownership validation.</param>
    /// <param name="workoutService">The workout service for ownership validation.</param>
    public StrategyController(
        IUnifiedStrategyService unifiedStrategyService,
        IBenchmarkService benchmarkService,
        IWorkoutService workoutService)
    {
        _unifiedStrategyService = unifiedStrategyService;
        _benchmarkService = benchmarkService;
        _workoutService = workoutService;
    }

    /// <summary>
    /// Get comprehensive workout strategy with all analysis data in a single request.
    /// </summary>
    /// <remarks>
    /// This endpoint combines pacing, volume load, time estimate, and strategy insights
    /// into a single deduplicated response. Use this instead of making 4 separate calls
    /// to the individual endpoints for better performance.
    ///
    /// Movement context (name, benchmark data, percentile) is returned once per movement
    /// in the `movementContexts` array and referenced by `movementDefinitionId` elsewhere.
    /// </remarks>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="workoutId">The workout's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Unified strategy response with pacing, volume load, time estimate, and insights.</returns>
    /// <response code="200">Returns comprehensive strategy analysis.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Athlete or workout not found, or doesn't belong to user.</response>
    [HttpGet("athletes/{athleteId:int}/workouts/{workoutId:int}/strategy")]
    [ProducesResponseType(typeof(WorkoutStrategyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkoutStrategyResponse>> GetWorkoutStrategy(
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

        var result = await _unifiedStrategyService.CalculateUnifiedStrategyAsync(athleteId, workoutId, ct);

        if (result is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Unable to calculate strategy. Ensure athlete has sufficient benchmark data."
            });
        }

        return Ok(result.ToResponse());
    }
}
