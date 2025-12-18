namespace WodStrat.Services.Dtos;

/// <summary>
/// Result wrapper for workout parsing operations.
/// Contains the parsed workout, validation status, and confidence scoring.
/// </summary>
public class ParsedWorkoutResult
{
    /// <summary>
    /// Whether parsing completed successfully with acceptable confidence.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The parsed workout data (null if parsing failed completely).
    /// </summary>
    public ParsedWorkoutDto? ParsedWorkout { get; set; }

    /// <summary>
    /// Overall confidence score (0-100).
    /// 100%: Perfect parse, all movements identified
    /// 80-99%: High confidence, minor uncertainties
    /// 60-79%: Medium confidence, some movements unrecognized
    /// Below 60%: Low confidence, significant issues
    /// </summary>
    public int ConfidenceScore { get; set; }

    /// <summary>
    /// Human-readable confidence level.
    /// </summary>
    public string ConfidenceLevel => ConfidenceScore switch
    {
        100 => "Perfect",
        >= 80 => "High",
        >= 60 => "Medium",
        _ => "Low"
    };

    /// <summary>
    /// List of parsing errors (blocking issues).
    /// </summary>
    public IList<ParsingErrorDto> Errors { get; set; } = new List<ParsingErrorDto>();

    /// <summary>
    /// List of parsing warnings (non-blocking issues).
    /// </summary>
    public IList<ParsingWarningDto> Warnings { get; set; } = new List<ParsingWarningDto>();

    /// <summary>
    /// Detailed breakdown of confidence factors.
    /// </summary>
    public ConfidenceBreakdown? ConfidenceDetails { get; set; }

    /// <summary>
    /// Indicates if the result is usable (Success or has recoverable warnings).
    /// </summary>
    public bool IsUsable => Success || (ParsedWorkout?.IsValid == true && ConfidenceScore >= 60);
}
