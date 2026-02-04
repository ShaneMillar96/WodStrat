using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WodStrat.Api.Mappings;
using WodStrat.Api.ViewModels.StrategyInsights;
using WodStrat.Services.Interfaces;

namespace WodStrat.Api.Controllers;

/// <summary>
/// API endpoints for workout strategy insights that combine pacing, volume load, and time estimate data.
/// </summary>
[ApiController]
[Route("api")]
[Produces("application/json")]
[Tags("StrategyInsights")]
[Authorize]
public class StrategyInsightsController : ControllerBase
{
    private readonly IStrategyInsightsService _strategyInsightsService;
    private readonly IBenchmarkService _benchmarkService;
    private readonly IWorkoutService _workoutService;

    /// <summary>
    /// Initializes a new instance of the <see cref="StrategyInsightsController"/> class.
    /// </summary>
    /// <param name="strategyInsightsService">The strategy insights service.</param>
    /// <param name="benchmarkService">The benchmark service for ownership validation.</param>
    /// <param name="workoutService">The workout service for ownership validation.</param>
    public StrategyInsightsController(
        IStrategyInsightsService strategyInsightsService,
        IBenchmarkService benchmarkService,
        IWorkoutService workoutService)
    {
        _strategyInsightsService = strategyInsightsService;
        _benchmarkService = benchmarkService;
        _workoutService = workoutService;
    }

    /// <summary>
    /// Get comprehensive strategy insights for a workout based on athlete's benchmarks.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="workoutId">The workout's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Strategy insights including difficulty score, confidence assessment, key focus movements, and risk alerts.</returns>
    /// <response code="200">Returns strategy insights.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Athlete or workout not found, or doesn't belong to user.</response>
    [HttpGet("athletes/{athleteId:int}/workouts/{workoutId:int}/strategy-insights")]
    [ProducesResponseType(typeof(StrategyInsightsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StrategyInsightsResponse>> GetStrategyInsights(
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

        var result = await _strategyInsightsService.CalculateStrategyInsightsAsync(athleteId, workoutId, ct);

        if (result is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Unable to calculate strategy insights. Ensure athlete has sufficient benchmark data."
            });
        }

        return Ok(result.ToResponse());
    }
}
