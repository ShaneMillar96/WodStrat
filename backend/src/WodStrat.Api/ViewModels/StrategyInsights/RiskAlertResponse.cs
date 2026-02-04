namespace WodStrat.Api.ViewModels.StrategyInsights;

/// <summary>
/// Response model for a risk alert.
/// </summary>
public class RiskAlertResponse
{
    /// <summary>
    /// Type of risk alert.
    /// </summary>
    /// <example>PacingMismatch</example>
    public string AlertType { get; set; } = string.Empty;

    /// <summary>
    /// Severity level of the alert (Info, Warning, Critical).
    /// </summary>
    /// <example>Info</example>
    public string Severity { get; set; } = string.Empty;

    /// <summary>
    /// Short title for the alert.
    /// </summary>
    /// <example>Mixed Strengths Detected</example>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed message explaining the risk.
    /// </summary>
    /// <example>You have both strong and weak movements in this workout.</example>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// List of movements affected by this alert.
    /// </summary>
    public List<string> AffectedMovements { get; set; } = new();

    /// <summary>
    /// Recommended action to address this risk.
    /// </summary>
    /// <example>Pace to your weakest movement to avoid early burnout</example>
    public string SuggestedAction { get; set; } = string.Empty;
}
