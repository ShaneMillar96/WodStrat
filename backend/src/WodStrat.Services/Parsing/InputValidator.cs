namespace WodStrat.Services.Parsing;

using System.Text.RegularExpressions;
using WodStrat.Services.Parsing.Errors;

/// <summary>
/// Validates workout text input before parsing.
/// </summary>
public static partial class InputValidator
{
    // Pattern to detect binary/encoded content
    [GeneratedRegex(@"[\x00-\x08\x0B\x0C\x0E-\x1F]", RegexOptions.Compiled)]
    private static partial Regex BinaryContentPattern();

    // Pattern for potentially dangerous characters (script tags, etc.)
    [GeneratedRegex(@"<[^>]*script|javascript:|data:", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex DangerousContentPattern();

    // Pattern to check for any numeric content
    [GeneratedRegex(@"\d", RegexOptions.Compiled)]
    private static partial Regex NumericContentPattern();

    /// <summary>
    /// Validates input text and returns any blocking errors.
    /// </summary>
    /// <param name="text">The raw input text.</param>
    /// <returns>Validation result with any errors found.</returns>
    public static InputValidationResult Validate(string? text)
    {
        var errors = new List<ParsingError>();
        var warnings = new List<ParsingError>();

        // Check for null/empty
        if (string.IsNullOrWhiteSpace(text))
        {
            errors.Add(ParsingErrorMessages.Create(
                ParsingErrorCode.EmptyInput,
                ParsingErrorSeverity.Error));

            return new InputValidationResult
            {
                IsValid = false,
                Errors = errors,
                Warnings = warnings,
                SanitizedText = string.Empty
            };
        }

        var trimmedText = text.Trim();

        // Check maximum length
        if (trimmedText.Length > ParsingErrorMessages.MaxInputLength)
        {
            errors.Add(ParsingErrorMessages.Create(
                ParsingErrorCode.InputTooLong,
                ParsingErrorSeverity.Error,
                messageArgs: ParsingErrorMessages.MaxInputLength));

            return new InputValidationResult
            {
                IsValid = false,
                Errors = errors,
                Warnings = warnings,
                SanitizedText = trimmedText
            };
        }

        // Check minimum length
        if (trimmedText.Length < ParsingErrorMessages.MinInputLength)
        {
            errors.Add(ParsingErrorMessages.Create(
                ParsingErrorCode.InputTooShort,
                ParsingErrorSeverity.Error,
                context: trimmedText));

            return new InputValidationResult
            {
                IsValid = false,
                Errors = errors,
                Warnings = warnings,
                SanitizedText = trimmedText
            };
        }

        // Check for binary content
        if (BinaryContentPattern().IsMatch(trimmedText))
        {
            errors.Add(ParsingErrorMessages.Create(
                ParsingErrorCode.BinaryContent,
                ParsingErrorSeverity.Error));

            return new InputValidationResult
            {
                IsValid = false,
                Errors = errors,
                Warnings = warnings,
                SanitizedText = trimmedText
            };
        }

        // Check for dangerous content
        if (DangerousContentPattern().IsMatch(trimmedText))
        {
            errors.Add(ParsingErrorMessages.Create(
                ParsingErrorCode.InvalidCharacters,
                ParsingErrorSeverity.Error));

            return new InputValidationResult
            {
                IsValid = false,
                Errors = errors,
                Warnings = warnings,
                SanitizedText = trimmedText
            };
        }

        // Warning: No numbers detected
        if (!NumericContentPattern().IsMatch(trimmedText))
        {
            warnings.Add(ParsingErrorMessages.Create(
                ParsingErrorCode.NoWorkoutStructure,
                ParsingErrorSeverity.Warning,
                context: "No numbers found in input - workout typically includes reps, duration, or distance."));
        }

        // Sanitize the text
        var sanitized = SanitizeText(trimmedText);

        return new InputValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings,
            SanitizedText = sanitized
        };
    }

    /// <summary>
    /// Sanitizes text by removing/replacing problematic characters.
    /// </summary>
    private static string SanitizeText(string text)
    {
        // Remove any control characters that slipped through
        var sanitized = BinaryContentPattern().Replace(text, "");

        // Normalize whitespace
        sanitized = Regex.Replace(sanitized, @"\s+", " ");

        return sanitized.Trim();
    }
}

/// <summary>
/// Result of input validation.
/// </summary>
public sealed class InputValidationResult
{
    /// <summary>
    /// Whether the input passed validation (no errors).
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Blocking errors found during validation.
    /// </summary>
    public IReadOnlyList<ParsingError> Errors { get; init; } = [];

    /// <summary>
    /// Non-blocking warnings found during validation.
    /// </summary>
    public IReadOnlyList<ParsingError> Warnings { get; init; } = [];

    /// <summary>
    /// The sanitized input text.
    /// </summary>
    public string SanitizedText { get; init; } = string.Empty;
}
