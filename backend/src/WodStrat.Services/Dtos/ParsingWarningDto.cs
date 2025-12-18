namespace WodStrat.Services.Dtos;

/// <summary>
/// Data transfer object for non-blocking parsing warnings.
/// </summary>
public class ParsingWarningDto
{
    /// <summary>
    /// Type of warning encountered.
    /// </summary>
    public string WarningType { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable warning message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Line number where the warning applies (0 for general warnings).
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// The original text that triggered the warning.
    /// </summary>
    public string? OriginalText { get; set; }

    /// <summary>
    /// Suggested fix or alternative.
    /// </summary>
    public string? Suggestion { get; set; }
}
