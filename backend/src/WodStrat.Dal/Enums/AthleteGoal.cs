using System.ComponentModel;

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
    ImprovePacing,

    /// <summary>
    /// CrossFit Open preparation.
    /// </summary>
    [Description("CrossFit Open preparation")]
    PrepareForOpen,

    /// <summary>
    /// General competition preparation.
    /// </summary>
    [Description("General competition preparation")]
    CompetitionPrep,

    /// <summary>
    /// Focus on strength development.
    /// </summary>
    [Description("Focus on strength development")]
    BuildStrength,

    /// <summary>
    /// Cardio/engine development.
    /// </summary>
    [Description("Cardio/engine development")]
    ImproveConditioning,

    /// <summary>
    /// Body composition goals.
    /// </summary>
    [Description("Body composition goals")]
    WeightManagement,

    /// <summary>
    /// Overall health and fitness.
    /// </summary>
    [Description("Overall health and fitness")]
    GeneralFitness
}
