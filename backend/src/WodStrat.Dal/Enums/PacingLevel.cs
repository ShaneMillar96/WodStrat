using System.ComponentModel;
using NpgsqlTypes;

namespace WodStrat.Dal.Enums;

/// <summary>
/// Recommended pacing intensity level for workout movements.
/// </summary>
public enum PacingLevel
{
    /// <summary>
    /// Athlete should pace conservatively (below 40th percentile).
    /// </summary>
    [Description("Pace conservatively, this is a weakness")]
    [PgName("light")]
    Light,

    /// <summary>
    /// Athlete can maintain moderate pace (40th-60th percentile).
    /// </summary>
    [Description("Maintain steady pace, average performance")]
    [PgName("moderate")]
    Moderate,

    /// <summary>
    /// Athlete can push harder (above 60th percentile).
    /// </summary>
    [Description("Push harder, this is a strength")]
    [PgName("heavy")]
    Heavy
}
