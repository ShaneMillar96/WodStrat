namespace WodStrat.Services.Interfaces;

/// <summary>
/// Service interface for extracting current user identity from HTTP context JWT claims.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current authenticated user's ID from JWT claims.
    /// </summary>
    /// <returns>User ID if authenticated, null otherwise.</returns>
    int? UserId { get; }

    /// <summary>
    /// Gets the current authenticated user's email from JWT claims.
    /// </summary>
    /// <returns>Email if authenticated, null otherwise.</returns>
    string? Email { get; }

    /// <summary>
    /// Indicates whether the current request is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the user ID or throws UnauthorizedAccessException if not authenticated.
    /// Use this when authentication is required.
    /// </summary>
    /// <returns>The authenticated user's ID.</returns>
    /// <exception cref="UnauthorizedAccessException">If user is not authenticated.</exception>
    int GetRequiredUserId();
}
