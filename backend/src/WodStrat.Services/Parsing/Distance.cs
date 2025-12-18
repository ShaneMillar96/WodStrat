using WodStrat.Dal.Enums;

namespace WodStrat.Services.Parsing;

/// <summary>
/// Represents a distance value with unit.
/// </summary>
/// <param name="Value">The numeric distance value.</param>
/// <param name="Unit">The unit of measurement (m, km, ft, mi).</param>
/// <param name="OriginalText">The original matched text.</param>
public sealed record Distance(
    decimal Value,
    DistanceUnit Unit,
    string OriginalText
)
{
    /// <summary>
    /// Converts distance to meters.
    /// </summary>
    public decimal ToMeters() => Unit switch
    {
        DistanceUnit.M => Value,
        DistanceUnit.Km => Value * 1000m,
        DistanceUnit.Ft => Value * 0.3048m,
        DistanceUnit.Mi => Value * 1609.344m,
        DistanceUnit.Cal => Value, // Calories don't convert to distance
        _ => Value
    };

    /// <summary>
    /// Converts distance to kilometers.
    /// </summary>
    public decimal ToKilometers() => ToMeters() / 1000m;

    /// <summary>
    /// Converts distance to miles.
    /// </summary>
    public decimal ToMiles() => ToMeters() / 1609.344m;
}
