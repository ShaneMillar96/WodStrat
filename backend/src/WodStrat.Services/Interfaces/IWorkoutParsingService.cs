using WodStrat.Services.Dtos;

namespace WodStrat.Services.Interfaces;

/// <summary>
/// Service interface for parsing workout text into structured data.
/// Core parsing engine that detects workout type, time domain, and movements.
/// </summary>
public interface IWorkoutParsingService
{
    /// <summary>
    /// Parses raw workout text into structured data without persisting.
    /// Use this for preview/validation before saving.
    /// </summary>
    /// <param name="workoutText">The raw workout text to parse.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Parsed workout result with detected type, movements, and any parsing errors.</returns>
    Task<ParsedWorkoutDto> ParseWorkoutTextAsync(string workoutText, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates workout text and returns parsing errors without full parsing.
    /// Lighter-weight operation for quick validation feedback.
    /// </summary>
    /// <param name="workoutText">The raw workout text to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of parsing errors, or empty list if valid.</returns>
    Task<IReadOnlyList<ParsingErrorDto>> ValidateWorkoutTextAsync(string workoutText, CancellationToken cancellationToken = default);
}
