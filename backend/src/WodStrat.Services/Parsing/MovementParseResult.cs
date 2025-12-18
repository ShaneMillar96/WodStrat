using WodStrat.Services.Dtos;

namespace WodStrat.Services.Parsing;

/// <summary>
/// Result of parsing a single movement line with identification status.
/// </summary>
public sealed class MovementParseResult
{
    /// <summary>
    /// Whether the movement was successfully parsed and identified.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The parsed movement DTO (null if parsing failed).
    /// </summary>
    public ParsedMovementDto? Movement { get; init; }

    /// <summary>
    /// Confidence in the parse result (0-100).
    /// </summary>
    public int Confidence { get; init; }

    /// <summary>
    /// Error if parsing failed.
    /// </summary>
    public ParsingErrorDto? Error { get; init; }

    /// <summary>
    /// Warning if parsing succeeded with issues.
    /// </summary>
    public ParsingWarningDto? Warning { get; init; }

    /// <summary>
    /// Original line number in the input.
    /// </summary>
    public int LineNumber { get; init; }

    /// <summary>
    /// Original line text.
    /// </summary>
    public string OriginalText { get; init; } = string.Empty;
}
