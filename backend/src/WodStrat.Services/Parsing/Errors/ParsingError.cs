namespace WodStrat.Services.Parsing.Errors;

/// <summary>
/// Represents a parsing error or warning with full context.
/// </summary>
public sealed class ParsingError
{
    /// <summary>
    /// The error code identifying the type of error.
    /// </summary>
    public ParsingErrorCode Code { get; init; }

    /// <summary>
    /// Human-readable error message (interpolated from template).
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Suggested fix or action for the user.
    /// </summary>
    public string? Suggestion { get; init; }

    /// <summary>
    /// Line number where the error occurred (1-indexed, null for general errors).
    /// </summary>
    public int? LineNumber { get; init; }

    /// <summary>
    /// The severity of this parsing issue.
    /// </summary>
    public ParsingErrorSeverity Severity { get; init; }

    /// <summary>
    /// Additional context (e.g., the original text, attempted value).
    /// </summary>
    public string? Context { get; init; }

    /// <summary>
    /// Similar movement suggestions (for UnknownMovement errors).
    /// </summary>
    public IReadOnlyList<string>? SimilarNames { get; init; }

    /// <summary>
    /// Factory method for creating error-severity issues.
    /// </summary>
    public static ParsingError CreateError(
        ParsingErrorCode code,
        string message,
        int? lineNumber = null,
        string? context = null,
        string? suggestion = null)
    {
        return new ParsingError
        {
            Code = code,
            Message = message,
            Severity = ParsingErrorSeverity.Error,
            LineNumber = lineNumber,
            Context = context,
            Suggestion = suggestion
        };
    }

    /// <summary>
    /// Factory method for creating warning-severity issues.
    /// </summary>
    public static ParsingError CreateWarning(
        ParsingErrorCode code,
        string message,
        int? lineNumber = null,
        string? context = null,
        string? suggestion = null,
        IReadOnlyList<string>? similarNames = null)
    {
        return new ParsingError
        {
            Code = code,
            Message = message,
            Severity = ParsingErrorSeverity.Warning,
            LineNumber = lineNumber,
            Context = context,
            Suggestion = suggestion,
            SimilarNames = similarNames
        };
    }

    /// <summary>
    /// Factory method for creating info-severity issues.
    /// </summary>
    public static ParsingError CreateInfo(
        ParsingErrorCode code,
        string message,
        int? lineNumber = null,
        string? context = null)
    {
        return new ParsingError
        {
            Code = code,
            Message = message,
            Severity = ParsingErrorSeverity.Info,
            LineNumber = lineNumber,
            Context = context
        };
    }
}
