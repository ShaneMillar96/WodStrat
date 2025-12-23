namespace WodStrat.Services.Parsing.Errors;

using System.ComponentModel;

/// <summary>
/// Severity levels for parsing issues.
/// </summary>
public enum ParsingErrorSeverity
{
    /// <summary>
    /// Blocking error - parsing cannot continue or result is unusable.
    /// </summary>
    [Description("Error")]
    Error = 0,

    /// <summary>
    /// Non-blocking warning - parsing continues with degraded confidence.
    /// </summary>
    [Description("Warning")]
    Warning = 1,

    /// <summary>
    /// Informational message - parsing unaffected.
    /// </summary>
    [Description("Info")]
    Info = 2
}
