using System.Text.RegularExpressions;

namespace WodStrat.Services.Parsing;

/// <summary>
/// Preprocesses workout text by normalizing and categorizing lines.
/// </summary>
public static partial class WorkoutTextPreprocessor
{
    // Pattern to detect workout titles/names (often first line, may have quotes or special format)
    [GeneratedRegex(@"^[""']?([A-Z][A-Za-z0-9\s\-'']+)[""']?$", RegexOptions.Compiled)]
    private static partial Regex WorkoutNamePattern();

    // Pattern for common named workouts
    [GeneratedRegex(@"^(Fran|Diane|Helen|Grace|Isabel|Karen|Mary|Cindy|Annie|Eva|Kelly|Linda|Nancy|Angie|Chelsea|Elizabeth|Filthy\s*Fifty|Fight\s*Gone\s*Bad|Murph|DT|Roy|Jackie)$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex NamedWorkoutPattern();

    /// <summary>
    /// Preprocesses raw workout text into normalized, categorized form.
    /// </summary>
    /// <param name="rawText">The raw workout text input.</param>
    /// <returns>Preprocessed workout text with categorized lines.</returns>
    public static PreprocessedWorkoutText Preprocess(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return new PreprocessedWorkoutText
            {
                OriginalText = rawText ?? string.Empty,
                IsEmpty = true
            };
        }

        // Step 1: Normalize line endings and clean text
        var normalizedText = NormalizeText(rawText);

        // Step 2: Split into lines and filter empty
        var allLines = normalizedText
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        if (allLines.Count == 0)
        {
            return new PreprocessedWorkoutText
            {
                OriginalText = rawText,
                NormalizedText = normalizedText,
                IsEmpty = true
            };
        }

        // Step 3: Extract workout name (if present)
        string? workoutName = null;
        var startIndex = 0;

        if (allLines.Count > 0)
        {
            var firstLine = allLines[0];
            // Check if first line is a named workout or looks like a title
            if (NamedWorkoutPattern().IsMatch(firstLine) ||
                (WorkoutNamePattern().IsMatch(firstLine) && !WorkoutPatterns.ContainsWorkoutType(firstLine)))
            {
                workoutName = firstLine.Trim('"', '\'', ' ');
                startIndex = 1;
            }
        }

        // Step 4: Categorize remaining lines
        var headerLines = new List<string>();
        var movementLines = new List<string>();

        for (var i = startIndex; i < allLines.Count; i++)
        {
            var line = allLines[i];

            if (WorkoutPatterns.IsHeaderLine(line))
            {
                headerLines.Add(line);
            }
            else
            {
                movementLines.Add(line);
            }
        }

        return new PreprocessedWorkoutText
        {
            OriginalText = rawText,
            NormalizedText = normalizedText,
            WorkoutName = workoutName,
            Lines = allLines.Skip(startIndex).ToList(),
            HeaderLines = headerLines,
            MovementLines = movementLines,
            IsEmpty = false
        };
    }

    /// <summary>
    /// Normalizes text by cleaning whitespace and special characters.
    /// </summary>
    private static string NormalizeText(string text)
    {
        // Normalize line endings to \n
        var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");

        // Replace multiple spaces with single space
        normalized = Regex.Replace(normalized, @"[ \t]+", " ");

        // Replace multiple newlines with double newline (paragraph break)
        normalized = Regex.Replace(normalized, @"\n{3,}", "\n\n");

        // Normalize common unicode characters
        normalized = normalized
            .Replace('\u2018', '\'') // Left single quote
            .Replace('\u2019', '\'') // Right single quote
            .Replace('\u201C', '"')  // Left double quote
            .Replace('\u201D', '"')  // Right double quote
            .Replace('\u2013', '-')  // En dash
            .Replace('\u2014', '-'); // Em dash

        return normalized.Trim();
    }
}
