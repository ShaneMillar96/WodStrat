namespace WodStrat.Services.Dtos;

/// <summary>
/// Data transfer object for workout parsing errors.
/// </summary>
public class ParsingErrorDto
{
    /// <summary>
    /// Type of error encountered (legacy string format for API compatibility).
    /// </summary>
    public string ErrorType { get; set; } = string.Empty;

    /// <summary>
    /// Numeric error code for programmatic handling.
    /// </summary>
    public int? ErrorCode { get; set; }

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Line number where the error occurred (0 for general errors).
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// The original text that caused the error.
    /// </summary>
    public string? OriginalText { get; set; }

    /// <summary>
    /// Suggested fix for the error.
    /// </summary>
    public string? Suggestion { get; set; }

    /// <summary>
    /// Similar movement name suggestions (for unknown movement errors).
    /// </summary>
    public IReadOnlyList<string>? SimilarNames { get; set; }
}
