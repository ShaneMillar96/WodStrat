using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WodStrat.Api.Mappings;
using WodStrat.Api.ViewModels.Auth;
using WodStrat.Services.Dtos.Auth;
using WodStrat.Services.Interfaces;

namespace WodStrat.Api.Controllers;

/// <summary>
/// API endpoints for user authentication.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
[Tags("Authentication")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user account.
    /// </summary>
    /// <param name="request">Registration credentials.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>JWT token and user info on success.</returns>
    /// <response code="201">Registration successful.</response>
    /// <response code="400">Validation errors.</response>
    /// <response code="409">Email already registered.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        var dto = request.ToDto();
        var result = await _authService.RegisterAsync(dto, ct);

        if (!result.Success)
        {
            return result.ErrorCode switch
            {
                AuthErrorCodes.EmailExists => Conflict(new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                    title = "Conflict",
                    status = 409,
                    detail = result.ErrorMessage
                }),
                AuthErrorCodes.PasswordMismatch => BadRequest(new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    title = "Bad Request",
                    status = 400,
                    detail = result.ErrorMessage
                }),
                _ => BadRequest(new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    title = "Bad Request",
                    status = 400,
                    detail = result.ErrorMessage
                })
            };
        }

        var response = result.Data!.ToResponse();
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Authenticate user and receive JWT token.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>JWT token and user info on success.</returns>
    /// <response code="200">Login successful.</response>
    /// <response code="400">Validation errors.</response>
    /// <response code="401">Invalid credentials.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var dto = request.ToDto();
        var result = await _authService.LoginAsync(dto, ct);

        if (!result.Success)
        {
            return result.ErrorCode switch
            {
                AuthErrorCodes.InvalidCredentials => Unauthorized(new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    title = "Unauthorized",
                    status = 401,
                    detail = result.ErrorMessage
                }),
                AuthErrorCodes.AccountDisabled => Unauthorized(new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    title = "Unauthorized",
                    status = 401,
                    detail = result.ErrorMessage
                }),
                _ => BadRequest(new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    title = "Bad Request",
                    status = 400,
                    detail = result.ErrorMessage
                })
            };
        }

        var response = result.Data!.ToResponse();
        return Ok(response);
    }
}
