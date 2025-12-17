using System.ComponentModel;
using NpgsqlTypes;

namespace WodStrat.Dal.Enums;

/// <summary>
/// Units for load/weight specification.
/// </summary>
public enum LoadUnit
{
    /// <summary>
    /// Kilograms.
    /// </summary>
    [Description("Kilograms")]
    [PgName("kg")]
    Kg,

    /// <summary>
    /// Pounds.
    /// </summary>
    [Description("Pounds")]
    [PgName("lb")]
    Lb,

    /// <summary>
    /// Russian kettlebell measurement (~16kg).
    /// </summary>
    [Description("Pood (~16kg)")]
    [PgName("pood")]
    Pood
}
