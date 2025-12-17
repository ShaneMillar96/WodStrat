using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WodStrat.Api.Mappings;
using WodStrat.Api.ViewModels.Workouts;
using WodStrat.Services.Interfaces;

namespace WodStrat.Api.Controllers;

/// <summary>
/// API endpoints for workout management and parsing.
/// </summary>
[ApiController]
[Route("api/workouts")]
[Produces("application/json")]
[Tags("Workouts")]
[Authorize]
public class WorkoutsController : ControllerBase
{
    private readonly IWorkoutService _workoutService;
    private readonly IWorkoutParsingService _workoutParsingService;

    public WorkoutsController(
        IWorkoutService workoutService,
        IWorkoutParsingService workoutParsingService)
    {
        _workoutService = workoutService;
        _workoutParsingService = workoutParsingService;
    }

    /// <summary>
    /// Parse workout text into structured data (preview without saving).
    /// </summary>
    /// <param name="request">The workout text to parse.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Parsed workout structure with detected type, movements, and any parsing errors.</returns>
    /// <response code="200">Successfully parsed workout text.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpPost("parse")]
    [ProducesResponseType(typeof(ParsedWorkoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ParsedWorkoutResponse>> ParseWorkoutText(
        [FromBody] ParseWorkoutRequest request,
        CancellationToken ct)
    {
        var result = await _workoutParsingService.ParseWorkoutTextAsync(request.Text, ct);
        return Ok(result.ToResponse());
    }

    /// <summary>
    /// Create a new workout.
    /// </summary>
    /// <param name="request">The workout data to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created workout.</returns>
    /// <response code="201">Workout created successfully.</response>
    /// <response code="400">Validation errors in request.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpPost]
    [ProducesResponseType(typeof(WorkoutResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<WorkoutResponse>> Create(
        [FromBody] CreateWorkoutRequest request,
        CancellationToken ct)
    {
        var dto = request.ToDto();
        var workout = await _workoutService.CreateWorkoutAsync(dto, ct);

        if (workout is null)
        {
            return Unauthorized(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Unauthorized",
                status = 401,
                detail = "Unable to create workout. User authentication required."
            });
        }

        var response = workout.ToResponse();
        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    /// <summary>
    /// Get all workouts for the authenticated user.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of user's workouts.</returns>
    /// <response code="200">Returns list of user's workouts.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WorkoutResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<WorkoutResponse>>> GetAll(CancellationToken ct)
    {
        var workouts = await _workoutService.GetCurrentUserWorkoutsAsync(ct);
        var response = workouts.Select(w => w.ToResponse());
        return Ok(response);
    }

    /// <summary>
    /// Get a specific workout by ID (ownership verified).
    /// </summary>
    /// <param name="id">The workout's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The workout.</returns>
    /// <response code="200">Returns the workout.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Workout not found or doesn't belong to user.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WorkoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkoutResponse>> GetById(
        int id,
        CancellationToken ct)
    {
        // First verify ownership
        var isOwner = await _workoutService.ValidateOwnershipAsync(id, ct);
        if (!isOwner)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Workout not found or doesn't belong to user."
            });
        }

        var workout = await _workoutService.GetWorkoutByIdAsync(id, ct);
        if (workout is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Workout not found or doesn't belong to user."
            });
        }

        return Ok(workout.ToResponse());
    }

    /// <summary>
    /// Update an existing workout (ownership verified).
    /// </summary>
    /// <param name="id">The workout's unique identifier.</param>
    /// <param name="request">The updated workout data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated workout.</returns>
    /// <response code="200">Workout updated successfully.</response>
    /// <response code="400">Validation errors in request.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Workout not found or doesn't belong to user.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(WorkoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkoutResponse>> Update(
        int id,
        [FromBody] UpdateWorkoutRequest request,
        CancellationToken ct)
    {
        var dto = request.ToDto();
        var workout = await _workoutService.UpdateWorkoutAsync(id, dto, ct);

        if (workout is null)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Workout not found or doesn't belong to user."
            });
        }

        return Ok(workout.ToResponse());
    }

    /// <summary>
    /// Soft delete a workout (ownership verified).
    /// </summary>
    /// <param name="id">The workout's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Workout deleted successfully.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Workout not found or doesn't belong to user.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken ct)
    {
        var success = await _workoutService.DeleteWorkoutAsync(id, ct);

        if (!success)
        {
            return NotFound(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "Workout not found or doesn't belong to user."
            });
        }

        return NoContent();
    }
}
