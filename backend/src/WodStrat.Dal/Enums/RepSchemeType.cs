using System.ComponentModel;
using NpgsqlTypes;

namespace WodStrat.Dal.Enums;

/// <summary>
/// Pattern type for workout rep schemes.
/// </summary>
public enum RepSchemeType
{
    /// <summary>
    /// Same reps each round (e.g., 10-10-10).
    /// </summary>
    [Description("Fixed reps per round")]
    [PgName("fixed")]
    Fixed,

    /// <summary>
    /// Decreasing reps each round (e.g., 21-15-9).
    /// </summary>
    [Description("Descending rep scheme")]
    [PgName("descending")]
    Descending,

    /// <summary>
    /// Increasing reps each round (e.g., 9-15-21).
    /// </summary>
    [Description("Ascending rep scheme")]
    [PgName("ascending")]
    Ascending,

    /// <summary>
    /// Custom pattern not fitting other categories (e.g., 1-2-3-4-5-4-3-2-1).
    /// </summary>
    [Description("Custom rep scheme")]
    [PgName("custom")]
    Custom
}
