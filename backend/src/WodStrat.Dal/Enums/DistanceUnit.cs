using System.ComponentModel;
using NpgsqlTypes;

namespace WodStrat.Dal.Enums;

/// <summary>
/// Units for distance specification.
/// </summary>
public enum DistanceUnit
{
    /// <summary>
    /// Meters.
    /// </summary>
    [Description("Meters")]
    [PgName("m")]
    M,

    /// <summary>
    /// Kilometers.
    /// </summary>
    [Description("Kilometers")]
    [PgName("km")]
    Km,

    /// <summary>
    /// Feet.
    /// </summary>
    [Description("Feet")]
    [PgName("ft")]
    Ft,

    /// <summary>
    /// Miles.
    /// </summary>
    [Description("Miles")]
    [PgName("mi")]
    Mi,

    /// <summary>
    /// Calories (for erg machines).
    /// </summary>
    [Description("Calories")]
    [PgName("cal")]
    Cal
}
