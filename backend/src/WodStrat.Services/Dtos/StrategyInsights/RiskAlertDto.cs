namespace WodStrat.Services.Dtos;

/// <summary>
/// Risk alert from strategy analysis.
/// </summary>
public class RiskAlertDto
{
    /// <summary>
    /// Type of risk alert.
    /// </summary>
    public string AlertType { get; set; } = string.Empty;

    /// <summary>
    /// Severity level (High, Medium, Low).
    /// </summary>
    public string Severity { get; set; } = string.Empty;

    /// <summary>
    /// Short title for the alert.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed message explaining the risk.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Movements affected by this risk.
    /// </summary>
    public IReadOnlyList<string> AffectedMovements { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Suggested action to mitigate the risk.
    /// </summary>
    public string SuggestedAction { get; set; } = string.Empty;
}
