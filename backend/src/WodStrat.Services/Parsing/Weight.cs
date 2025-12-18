using WodStrat.Dal.Enums;

namespace WodStrat.Services.Parsing;

/// <summary>
/// Represents a weight/load value with unit.
/// </summary>
/// <param name="Value">The numeric weight value.</param>
/// <param name="Unit">The unit of measurement (kg, lb, pood).</param>
/// <param name="OriginalText">The original matched text.</param>
public sealed record Weight(
    decimal Value,
    LoadUnit Unit,
    string OriginalText
)
{
    /// <summary>
    /// Converts weight to kilograms.
    /// </summary>
    public decimal ToKg() => Unit switch
    {
        LoadUnit.Kg => Value,
        LoadUnit.Lb => Value * 0.453592m,
        LoadUnit.Pood => Value * 16.38m,
        _ => Value
    };

    /// <summary>
    /// Converts weight to pounds.
    /// </summary>
    public decimal ToLb() => Unit switch
    {
        LoadUnit.Lb => Value,
        LoadUnit.Kg => Value * 2.20462m,
        LoadUnit.Pood => Value * 36.11m,
        _ => Value
    };
}

/// <summary>
/// Represents a gender-differentiated weight pair (RX/Scaled).
/// </summary>
/// <param name="Male">The male (heavier) weight.</param>
/// <param name="Female">The female (lighter) weight.</param>
/// <param name="OriginalText">The original matched text.</param>
public sealed record WeightPair(
    Weight Male,
    Weight Female,
    string OriginalText
);

/// <summary>
/// Represents a percentage-based load.
/// </summary>
/// <param name="Percentage">The percentage value (0-100 or higher for PRs).</param>
/// <param name="Reference">What the percentage is of (e.g., "1RM", "bodyweight").</param>
/// <param name="OriginalText">The original matched text.</param>
public sealed record PercentageLoad(
    int Percentage,
    string? Reference,
    string OriginalText
);
