namespace WodStrat.Services.Dtos.Auth;

/// <summary>
/// Generic result wrapper for authentication operations (success/failure).
/// </summary>
/// <typeparam name="T">The type of data returned on success.</typeparam>
public class AuthResult<T>
{
    /// <summary>
    /// Indicates whether the operation succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The result data if successful.
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Error code for failed operations.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates a successful result with data.
    /// </summary>
    /// <param name="data">The result data.</param>
    /// <returns>A successful AuthResult.</returns>
    public static AuthResult<T> Succeed(T data) => new()
    {
        Success = true,
        Data = data
    };

    /// <summary>
    /// Creates a failed result with error details.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <param name="errorMessage">Human-readable error message.</param>
    /// <returns>A failed AuthResult.</returns>
    public static AuthResult<T> Fail(string errorCode, string errorMessage) => new()
    {
        Success = false,
        ErrorCode = errorCode,
        ErrorMessage = errorMessage
    };
}

/// <summary>
/// Common authentication error codes.
/// </summary>
public static class AuthErrorCodes
{
    /// <summary>
    /// Email already registered.
    /// </summary>
    public const string EmailExists = "EMAIL_EXISTS";

    /// <summary>
    /// Email/password combination invalid.
    /// </summary>
    public const string InvalidCredentials = "INVALID_CREDENTIALS";

    /// <summary>
    /// User account is not active.
    /// </summary>
    public const string AccountDisabled = "ACCOUNT_DISABLED";

    /// <summary>
    /// Confirm password doesn't match.
    /// </summary>
    public const string PasswordMismatch = "PASSWORD_MISMATCH";
}
