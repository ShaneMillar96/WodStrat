using System.ComponentModel;
using NpgsqlTypes;

namespace WodStrat.Dal.Enums;

/// <summary>
/// Category classification for movement types.
/// </summary>
public enum MovementCategory
{
    /// <summary>
    /// Olympic lifts and barbell movements.
    /// </summary>
    [Description("Weightlifting movements")]
    [PgName("weightlifting")]
    Weightlifting,

    /// <summary>
    /// Bodyweight and gymnastics movements.
    /// </summary>
    [Description("Gymnastics movements")]
    [PgName("gymnastics")]
    Gymnastics,

    /// <summary>
    /// Cardiovascular/monostructural movements.
    /// </summary>
    [Description("Cardio movements")]
    [PgName("cardio")]
    Cardio,

    /// <summary>
    /// Strongman implements and carries.
    /// </summary>
    [Description("Strongman movements")]
    [PgName("strongman")]
    Strongman
}
