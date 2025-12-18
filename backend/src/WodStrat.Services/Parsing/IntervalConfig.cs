namespace WodStrat.Services.Parsing;

/// <summary>
/// Represents an interval training configuration.
/// </summary>
/// <param name="Rounds">Number of rounds/intervals.</param>
/// <param name="WorkSeconds">Work period duration in seconds.</param>
/// <param name="RestSeconds">Rest period duration in seconds.</param>
/// <param name="OriginalText">The original matched text.</param>
public sealed record IntervalConfig(
    int Rounds,
    int WorkSeconds,
    int RestSeconds,
    string OriginalText
)
{
    /// <summary>
    /// Total workout duration in seconds.
    /// </summary>
    public int TotalSeconds => Rounds * (WorkSeconds + RestSeconds);

    /// <summary>
    /// Work-to-rest ratio.
    /// </summary>
    public decimal WorkRestRatio => RestSeconds > 0 ? (decimal)WorkSeconds / RestSeconds : decimal.MaxValue;
}
