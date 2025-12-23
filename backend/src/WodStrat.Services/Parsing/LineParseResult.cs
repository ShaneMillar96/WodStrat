namespace WodStrat.Services.Parsing;

using WodStrat.Services.Dtos;
using WodStrat.Services.Parsing.Errors;

/// <summary>
/// Result of parsing a single line with detailed tracking.
/// </summary>
public sealed class LineParseResult
{
    /// <summary>
    /// The original line number (1-indexed).
    /// </summary>
    public int LineNumber { get; init; }

    /// <summary>
    /// The original text of the line.
    /// </summary>
    public string OriginalText { get; init; } = string.Empty;

    /// <summary>
    /// Whether this line was successfully parsed.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The parsed movement data (null if parsing failed).
    /// </summary>
    public ParsedMovementDto? ParsedMovement { get; init; }

    /// <summary>
    /// Any error that occurred during parsing.
    /// </summary>
    public ParsingError? Error { get; init; }

    /// <summary>
    /// Any warnings from parsing this line.
    /// </summary>
    public IReadOnlyList<ParsingError> Warnings { get; init; } = [];

    /// <summary>
    /// Confidence in this line's parse result (0-100).
    /// </summary>
    public int Confidence { get; init; }

    /// <summary>
    /// Whether this line was skipped (e.g., header line, empty).
    /// </summary>
    public bool Skipped { get; init; }

    /// <summary>
    /// Reason for skipping this line.
    /// </summary>
    public string? SkipReason { get; init; }

    /// <summary>
    /// Creates a successful parse result.
    /// </summary>
    public static LineParseResult CreateSuccess(
        int lineNumber,
        string originalText,
        ParsedMovementDto movement,
        int confidence,
        IReadOnlyList<ParsingError>? warnings = null)
    {
        return new LineParseResult
        {
            LineNumber = lineNumber,
            OriginalText = originalText,
            Success = true,
            ParsedMovement = movement,
            Confidence = confidence,
            Warnings = warnings ?? []
        };
    }

    /// <summary>
    /// Creates a failed parse result.
    /// </summary>
    public static LineParseResult CreateFailure(
        int lineNumber,
        string originalText,
        ParsingError error)
    {
        return new LineParseResult
        {
            LineNumber = lineNumber,
            OriginalText = originalText,
            Success = false,
            Error = error,
            Confidence = 0
        };
    }

    /// <summary>
    /// Creates a skipped line result.
    /// </summary>
    public static LineParseResult CreateSkipped(
        int lineNumber,
        string originalText,
        string reason)
    {
        return new LineParseResult
        {
            LineNumber = lineNumber,
            OriginalText = originalText,
            Success = false,
            Skipped = true,
            SkipReason = reason,
            Confidence = 0
        };
    }
}
