namespace WodStrat.Services.Parsing.Errors;

/// <summary>
/// Centralized repository of error message templates.
/// </summary>
public static class ParsingErrorMessages
{
    /// <summary>
    /// Maximum input length in characters.
    /// </summary>
    public const int MaxInputLength = 10000;

    /// <summary>
    /// Minimum input length in characters.
    /// </summary>
    public const int MinInputLength = 5;

    /// <summary>
    /// Maximum number of errors to report.
    /// </summary>
    public const int MaxErrorCount = 20;

    /// <summary>
    /// Number of similar movement suggestions to provide.
    /// </summary>
    public const int SimilarNameSuggestionCount = 3;

    /// <summary>
    /// Error message templates keyed by error code.
    /// Use {0}, {1}, etc. for interpolation.
    /// </summary>
    private static readonly Dictionary<ParsingErrorCode, (string Template, string Suggestion)> Messages = new()
    {
        // Input validation
        [ParsingErrorCode.EmptyInput] = (
            "Workout text cannot be empty.",
            "Enter a workout description including movements and quantities."),

        [ParsingErrorCode.InputTooLong] = (
            "Workout text exceeds maximum length of {0:N0} characters.",
            "Reduce the workout description or split into multiple workouts."),

        [ParsingErrorCode.InputTooShort] = (
            "Workout text is too short to contain valid workout data.",
            "Include at least one movement with reps, distance, or duration."),

        [ParsingErrorCode.BinaryContent] = (
            "Input appears to contain binary or encoded content.",
            "Paste plain text workout description only."),

        [ParsingErrorCode.InvalidCharacters] = (
            "Input contains potentially harmful characters.",
            "Remove special characters and use plain text."),

        // Structural
        [ParsingErrorCode.NoWorkoutStructure] = (
            "Could not detect a valid workout structure.",
            "Include workout type (e.g., 'AMRAP 20 min', 'For Time', '5 Rounds')."),

        [ParsingErrorCode.NoMovementsDetected] = (
            "No movements could be parsed from the workout text.",
            "List movements with quantities (e.g., '21 Thrusters', '400m Run')."),

        [ParsingErrorCode.InvalidWorkoutType] = (
            "'{0}' is not a recognized workout type.",
            "Use standard types: AMRAP, For Time, EMOM, Rounds, Tabata, Intervals."),

        [ParsingErrorCode.AmbiguousWorkoutType] = (
            "Multiple workout types detected: {0}.",
            "Specify a single workout type clearly."),

        [ParsingErrorCode.MissingDuration] = (
            "Timed workout (AMRAP/EMOM) requires a duration.",
            "Add duration (e.g., '20 min AMRAP', 'EMOM x 10 minutes')."),

        [ParsingErrorCode.MissingRoundCount] = (
            "Rounds-based workout requires a round count.",
            "Specify rounds (e.g., '5 Rounds for Time')."),

        [ParsingErrorCode.ContradictoryMetadata] = (
            "Contradictory workout metadata: {0}.",
            "Review and correct conflicting information."),

        // Movement parsing
        [ParsingErrorCode.UnknownMovement] = (
            "Movement '{0}' not recognized.",
            "Check spelling or try a common abbreviation."),

        [ParsingErrorCode.AmbiguousMovement] = (
            "'{0}' could match multiple movements: {1}.",
            "Use the full movement name or common abbreviation."),

        [ParsingErrorCode.InvalidRepCount] = (
            "Invalid rep count '{0}'.",
            "Use a positive whole number for reps."),

        [ParsingErrorCode.InvalidWeight] = (
            "Invalid weight '{0}'.",
            "Use format like '135 lbs', '60 kg', or '1.5 pood'."),

        [ParsingErrorCode.InvalidDistance] = (
            "Invalid distance '{0}'.",
            "Use format like '400m', '1 mile', or '5k'."),

        [ParsingErrorCode.InvalidTime] = (
            "Invalid time value '{0}'.",
            "Use format like '2:00', '90 sec', or '3 min'."),

        [ParsingErrorCode.InvalidCalories] = (
            "Invalid calorie value '{0}'.",
            "Use a positive whole number for calories."),

        [ParsingErrorCode.EmptyMovementLine] = (
            "Movement line is empty.",
            "Remove empty lines or add movement details."),

        [ParsingErrorCode.UnrecognizedMovementFormat] = (
            "Could not parse movement: '{0}'.",
            "Use format: quantity + movement (e.g., '21 Thrusters')."),

        // Data consistency
        [ParsingErrorCode.DuplicateMovement] = (
            "Movement '{0}' appears multiple times in sequence.",
            "Intentional duplicates are allowed but flagged for review."),

        [ParsingErrorCode.InconsistentUnits] = (
            "Inconsistent units: {0}.",
            "Use consistent units throughout the workout."),

        [ParsingErrorCode.ValueOutOfRange] = (
            "Value '{0}' is outside reasonable range.",
            "Verify the value is correct."),

        // System
        [ParsingErrorCode.InternalError] = (
            "An internal parsing error occurred.",
            "Please try again or report this issue."),

        [ParsingErrorCode.Timeout] = (
            "Parsing timed out.",
            "Simplify the workout text or try again.")
    };

    /// <summary>
    /// Gets a formatted error message for the given code.
    /// </summary>
    public static string GetMessage(ParsingErrorCode code, params object[] args)
    {
        if (!Messages.TryGetValue(code, out var template))
        {
            return $"Unknown error: {code}";
        }

        try
        {
            return args.Length > 0
                ? string.Format(template.Template, args)
                : template.Template;
        }
        catch (FormatException)
        {
            return template.Template;
        }
    }

    /// <summary>
    /// Gets the suggestion for the given error code.
    /// </summary>
    public static string GetSuggestion(ParsingErrorCode code)
    {
        return Messages.TryGetValue(code, out var template)
            ? template.Suggestion
            : string.Empty;
    }

    /// <summary>
    /// Creates a ParsingError with message and suggestion from templates.
    /// </summary>
    public static ParsingError Create(
        ParsingErrorCode code,
        ParsingErrorSeverity severity,
        int? lineNumber = null,
        string? context = null,
        IReadOnlyList<string>? similarNames = null,
        params object[] messageArgs)
    {
        return new ParsingError
        {
            Code = code,
            Message = GetMessage(code, messageArgs),
            Suggestion = GetSuggestion(code),
            Severity = severity,
            LineNumber = lineNumber,
            Context = context,
            SimilarNames = similarNames
        };
    }
}
