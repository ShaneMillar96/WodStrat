namespace WodStrat.Services.Parsing;

/// <summary>
/// Result of text preprocessing stage.
/// </summary>
public sealed class PreprocessedWorkoutText
{
    /// <summary>
    /// Original input text.
    /// </summary>
    public string OriginalText { get; init; } = string.Empty;

    /// <summary>
    /// Cleaned and normalized text.
    /// </summary>
    public string NormalizedText { get; init; } = string.Empty;

    /// <summary>
    /// Extracted workout name/title (if present).
    /// </summary>
    public string? WorkoutName { get; init; }

    /// <summary>
    /// Individual lines after normalization (non-empty).
    /// </summary>
    public IReadOnlyList<string> Lines { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Header lines (workout type, time cap, rep scheme).
    /// </summary>
    public IReadOnlyList<string> HeaderLines { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Movement lines (to be parsed).
    /// </summary>
    public IReadOnlyList<string> MovementLines { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Whether the input was empty or whitespace-only.
    /// </summary>
    public bool IsEmpty { get; init; }

    /// <summary>
    /// Workout-level rep scheme (applies to all movements unless overridden).
    /// </summary>
    public RepScheme? WorkoutRepScheme { get; init; }

    /// <summary>
    /// Movement-specific rep scheme assignments (for complex cases).
    /// Key = index in MovementLines list.
    /// </summary>
    public IReadOnlyDictionary<int, RepScheme> MovementRepSchemes { get; init; }
        = new Dictionary<int, RepScheme>();
}
