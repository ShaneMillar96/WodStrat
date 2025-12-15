using System.ComponentModel;
using NpgsqlTypes;

namespace WodStrat.Dal.Enums;

/// <summary>
/// Represents the athlete's primary training goal.
/// </summary>
public enum AthleteGoal
{
    /// <summary>
    /// Better workout pacing and consistency.
    /// </summary>
    [Description("Better workout pacing and consistency")]
    [PgName("improve_pacing")]
    ImprovePacing,

    /// <summary>
    /// CrossFit Open preparation.
    /// </summary>
    [Description("CrossFit Open preparation")]
    [PgName("prepare_for_open")]
    PrepareForOpen,

    /// <summary>
    /// General competition preparation.
    /// </summary>
    [Description("General competition preparation")]
    [PgName("competition_prep")]
    CompetitionPrep,

    /// <summary>
    /// Focus on strength development.
    /// </summary>
    [Description("Focus on strength development")]
    [PgName("build_strength")]
    BuildStrength,

    /// <summary>
    /// Cardio/engine development.
    /// </summary>
    [Description("Cardio/engine development")]
    [PgName("improve_conditioning")]
    ImproveConditioning,

    /// <summary>
    /// Body composition goals.
    /// </summary>
    [Description("Body composition goals")]
    [PgName("weight_management")]
    WeightManagement,

    /// <summary>
    /// Overall health and fitness.
    /// </summary>
    [Description("Overall health and fitness")]
    [PgName("general_fitness")]
    GeneralFitness
}
