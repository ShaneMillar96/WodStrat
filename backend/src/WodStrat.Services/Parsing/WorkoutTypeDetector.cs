using WodStrat.Dal.Enums;
using WodStrat.Services.Dtos;
using WodStrat.Services.Interfaces;

namespace WodStrat.Services.Parsing;

/// <summary>
/// Detects workout type from preprocessed text with confidence scoring.
/// </summary>
public class WorkoutTypeDetector
{
    private readonly IPatternMatchingService _patternMatchingService;

    public WorkoutTypeDetector(IPatternMatchingService patternMatchingService)
    {
        _patternMatchingService = patternMatchingService;
    }

    /// <summary>
    /// Detects the workout type from preprocessed text.
    /// </summary>
    /// <param name="preprocessed">The preprocessed workout text.</param>
    /// <returns>Workout type detection result with confidence.</returns>
    public WorkoutTypeDetectionResult Detect(PreprocessedWorkoutText preprocessed)
    {
        if (preprocessed.IsEmpty)
        {
            return new WorkoutTypeDetectionResult
            {
                Type = WorkoutType.ForTime,
                Confidence = 0,
                Error = new ParsingErrorDto
                {
                    ErrorType = "EmptyInput",
                    Message = "Workout text cannot be empty.",
                    LineNumber = 0
                }
            };
        }

        // Combine header lines for type detection
        var headerText = string.Join("\n", preprocessed.HeaderLines);
        var fullText = string.Join("\n", preprocessed.Lines);

        // Use pattern matching service
        var match = _patternMatchingService.DetectWorkoutType(
            string.IsNullOrWhiteSpace(headerText) ? fullText : headerText);

        // Check for rep scheme in text (implies For Time if no other type)
        var repScheme = _patternMatchingService.ExtractRepScheme(fullText);

        // Extract time cap
        var timeCap = _patternMatchingService.ExtractTimeCap(fullText);

        // Refine detection based on additional context
        var result = new WorkoutTypeDetectionResult
        {
            Type = match.Type,
            TimeCapSeconds = match.TimeCapSeconds ?? (timeCap.HasValue ? (int)timeCap.Value.TotalSeconds : null),
            RoundCount = match.RoundCount,
            IntervalSeconds = match.IntervalSeconds,
            Confidence = (int)(match.Confidence * 100),
            MatchedPattern = match.MatchedPattern,
            RepScheme = repScheme
        };

        // Adjust confidence based on context
        if (result.Type == WorkoutType.ForTime && timeCap.HasValue)
        {
            result.Confidence = Math.Max(result.Confidence, 90);
        }

        if (result.Type == WorkoutType.Amrap && !result.TimeCapSeconds.HasValue)
        {
            // AMRAP without duration is suspicious
            result.Confidence = Math.Min(result.Confidence, 70);
            result.Warning = new ParsingWarningDto
            {
                WarningType = "MissingDuration",
                Message = "AMRAP workout detected without duration. Consider adding a time cap.",
                Suggestion = "Add duration like '20 min AMRAP'"
            };
        }

        return result;
    }
}

/// <summary>
/// Result of workout type detection.
/// </summary>
public class WorkoutTypeDetectionResult
{
    public WorkoutType Type { get; set; }
    public int? TimeCapSeconds { get; set; }
    public int? RoundCount { get; set; }
    public int? IntervalSeconds { get; set; }
    public int Confidence { get; set; }
    public string? MatchedPattern { get; set; }
    public RepScheme? RepScheme { get; set; }
    public ParsingErrorDto? Error { get; set; }
    public ParsingWarningDto? Warning { get; set; }
}
