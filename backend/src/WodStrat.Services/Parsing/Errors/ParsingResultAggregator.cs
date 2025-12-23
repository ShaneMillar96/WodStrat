namespace WodStrat.Services.Parsing.Errors;

/// <summary>
/// Aggregates and summarizes parsing errors and warnings.
/// </summary>
public class ParsingResultAggregator
{
    private readonly List<ParsingError> _errors = [];
    private readonly List<ParsingError> _warnings = [];
    private readonly List<ParsingError> _info = [];
    private readonly HashSet<string> _seenErrorKeys = [];

    /// <summary>
    /// Maximum number of errors to report.
    /// </summary>
    public int MaxErrors { get; init; } = ParsingErrorMessages.MaxErrorCount;

    /// <summary>
    /// All collected errors (blocking issues).
    /// </summary>
    public IReadOnlyList<ParsingError> Errors => _errors;

    /// <summary>
    /// All collected warnings (non-blocking issues).
    /// </summary>
    public IReadOnlyList<ParsingError> Warnings => _warnings;

    /// <summary>
    /// All collected info messages.
    /// </summary>
    public IReadOnlyList<ParsingError> Info => _info;

    /// <summary>
    /// Whether the error limit has been reached.
    /// </summary>
    public bool ErrorLimitReached => _errors.Count >= MaxErrors;

    /// <summary>
    /// Total count of all issues.
    /// </summary>
    public int TotalIssueCount => _errors.Count + _warnings.Count + _info.Count;

    /// <summary>
    /// Adds a parsing error/warning/info to the collection.
    /// Deduplicates similar errors.
    /// </summary>
    /// <param name="error">The parsing error to add.</param>
    /// <returns>True if added, false if deduplicated/limited.</returns>
    public bool Add(ParsingError error)
    {
        // Check for duplicates
        var key = GetDeduplicationKey(error);
        if (_seenErrorKeys.Contains(key))
        {
            return false;
        }
        _seenErrorKeys.Add(key);

        // Route to appropriate list
        switch (error.Severity)
        {
            case ParsingErrorSeverity.Error:
                if (_errors.Count >= MaxErrors)
                {
                    return false;
                }
                _errors.Add(error);
                break;

            case ParsingErrorSeverity.Warning:
                _warnings.Add(error);
                break;

            case ParsingErrorSeverity.Info:
                _info.Add(error);
                break;
        }

        return true;
    }

    /// <summary>
    /// Adds multiple errors.
    /// </summary>
    public void AddRange(IEnumerable<ParsingError> errors)
    {
        foreach (var error in errors)
        {
            Add(error);
        }
    }

    /// <summary>
    /// Generates a deduplication key for an error.
    /// Errors with same code and similar context are considered duplicates.
    /// </summary>
    private static string GetDeduplicationKey(ParsingError error)
    {
        // For line-specific errors, include line number
        if (error.LineNumber.HasValue)
        {
            return $"{error.Code}:{error.LineNumber}";
        }

        // For general errors, use code + first 50 chars of context
        var contextKey = error.Context?.Length > 50
            ? error.Context[..50]
            : error.Context ?? "";
        return $"{error.Code}:{contextKey}";
    }

    /// <summary>
    /// Gets sorted errors by severity (errors first, then warnings, then info).
    /// </summary>
    public IReadOnlyList<ParsingError> GetSortedIssues()
    {
        return [.. _errors, .. _warnings, .. _info];
    }

    /// <summary>
    /// Gets a summary of the parsing issues.
    /// </summary>
    public ParsingIssueSummary GetSummary()
    {
        return new ParsingIssueSummary
        {
            ErrorCount = _errors.Count,
            WarningCount = _warnings.Count,
            InfoCount = _info.Count,
            ErrorLimitReached = ErrorLimitReached,
            ErrorsByCode = _errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Count()),
            WarningsByCode = _warnings
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    /// <summary>
    /// Clears all collected errors.
    /// </summary>
    public void Clear()
    {
        _errors.Clear();
        _warnings.Clear();
        _info.Clear();
        _seenErrorKeys.Clear();
    }
}

/// <summary>
/// Summary statistics for parsing issues.
/// </summary>
public sealed class ParsingIssueSummary
{
    public int ErrorCount { get; init; }
    public int WarningCount { get; init; }
    public int InfoCount { get; init; }
    public bool ErrorLimitReached { get; init; }
    public Dictionary<ParsingErrorCode, int> ErrorsByCode { get; init; } = [];
    public Dictionary<ParsingErrorCode, int> WarningsByCode { get; init; } = [];

    public int TotalIssueCount => ErrorCount + WarningCount + InfoCount;
}
