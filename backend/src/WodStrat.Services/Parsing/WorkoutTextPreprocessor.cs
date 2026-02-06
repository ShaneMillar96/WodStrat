using System.Text.RegularExpressions;
using WodStrat.Dal.Enums;

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

        // Step 4: Categorize remaining lines and track rep schemes
        var headerLines = new List<string>();
        var movementLines = new List<string>();
        RepScheme? pendingRepScheme = null;
        RepScheme? workoutRepScheme = null;
        var movementRepSchemes = new Dictionary<int, RepScheme>();

        for (var i = startIndex; i < allLines.Count; i++)
        {
            var line = allLines[i];

            // Check if this is a rep scheme line (e.g., "21-15-9" or "21-15-9 reps")
            var cleanedLine = line.Replace(" reps", "", StringComparison.OrdinalIgnoreCase)
                                  .Replace(" Reps", "", StringComparison.OrdinalIgnoreCase)
                                  .Trim();
            var repSchemeMatch = WorkoutPatterns.ChipperRepSchemePattern().Match(cleanedLine);
            if (repSchemeMatch.Success)
            {
                var extracted = ExtractRepSchemeFromMatch(repSchemeMatch.Groups[1].Value);
                if (extracted != null)
                {
                    pendingRepScheme = extracted;
                    headerLines.Add(line);

                    // If this is the first rep scheme, it may be workout-level
                    workoutRepScheme ??= extracted;
                    continue;
                }
            }

            if (WorkoutPatterns.IsHeaderLine(line))
            {
                headerLines.Add(line);
            }
            else
            {
                // This is a movement line
                var movementIndex = movementLines.Count;
                movementLines.Add(line);

                // If we have a pending rep scheme, assign it to this movement
                if (pendingRepScheme != null)
                {
                    movementRepSchemes[movementIndex] = pendingRepScheme;
                    pendingRepScheme = null;
                }
            }
        }

        // Determine workout-level rep scheme
        // If no movement-specific schemes, use the first detected rep scheme as workout-level
        // If all movements have the same rep scheme, treat it as workout-level
        if (movementRepSchemes.Count > 0 && movementRepSchemes.Values.Distinct().Count() == 1)
        {
            // All movements have same rep scheme - treat as workout-level
            workoutRepScheme = movementRepSchemes.Values.First();
            movementRepSchemes.Clear();
        }
        else if (movementRepSchemes.Count > 0)
        {
            // Multiple different rep schemes - clear workout level, keep movement-specific
            workoutRepScheme = null;
        }

        return new PreprocessedWorkoutText
        {
            OriginalText = rawText,
            NormalizedText = normalizedText,
            WorkoutName = workoutName,
            Lines = allLines.Skip(startIndex).ToList(),
            HeaderLines = headerLines,
            MovementLines = movementLines,
            WorkoutRepScheme = workoutRepScheme,
            MovementRepSchemes = movementRepSchemes,
            IsEmpty = false
        };
    }

    /// <summary>
    /// Extracts a RepScheme from a matched rep scheme string (e.g., "21-15-9").
    /// </summary>
    private static RepScheme? ExtractRepSchemeFromMatch(string repSchemeText)
    {
        var parts = repSchemeText.Split('-');
        if (parts.Length < 2) return null;

        var reps = new List<int>();
        foreach (var part in parts)
        {
            if (int.TryParse(part.Trim(), out var rep))
            {
                reps.Add(rep);
            }
            else
            {
                return null;
            }
        }

        if (reps.Count < 2) return null;

        var type = DetermineRepSchemeType(reps);
        return new RepScheme(reps, type, repSchemeText);
    }

    /// <summary>
    /// Determines the rep scheme type based on the sequence of reps.
    /// </summary>
    private static Dal.Enums.RepSchemeType DetermineRepSchemeType(IReadOnlyList<int> reps)
    {
        if (reps.Count <= 1) return Dal.Enums.RepSchemeType.Fixed;

        var isDescending = true;
        var isAscending = true;
        var isFixed = true;

        for (var i = 1; i < reps.Count; i++)
        {
            if (reps[i] >= reps[i - 1]) isDescending = false;
            if (reps[i] <= reps[i - 1]) isAscending = false;
            if (reps[i] != reps[0]) isFixed = false;
        }

        if (isFixed) return Dal.Enums.RepSchemeType.Fixed;
        if (isDescending) return Dal.Enums.RepSchemeType.Descending;
        if (isAscending) return Dal.Enums.RepSchemeType.Ascending;
        return Dal.Enums.RepSchemeType.Custom;
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
