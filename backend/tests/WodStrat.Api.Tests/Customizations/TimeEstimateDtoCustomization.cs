using AutoFixture;
using WodStrat.Services.Dtos;

namespace WodStrat.Api.Tests.Customizations;

/// <summary>
/// AutoFixture customization for creating valid time estimate DTOs.
/// </summary>
public class TimeEstimateDtoCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        // Replace ThrowingRecursionBehavior with OmitOnRecursionBehavior to handle circular references
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // Customize TimeEstimateResultDto
        fixture.Customize<TimeEstimateResultDto>(c => c
            .With(x => x.WorkoutId, () => fixture.Create<int>())
            .With(x => x.WorkoutName, "Test Workout")
            .With(x => x.WorkoutType, "ForTime")
            .With(x => x.EstimateType, "Time")
            .With(x => x.MinEstimate, 480)
            .With(x => x.MaxEstimate, 600)
            .With(x => x.MinExtraReps, (int?)null)
            .With(x => x.MaxExtraReps, (int?)null)
            .With(x => x.FormattedRange, "8:00 - 10:00")
            .With(x => x.ConfidenceLevel, TimeEstimateConfidenceLevel.High)
            .With(x => x.FactorsSummary, "Intermediate experience. High benchmark coverage.")
            .With(x => x.RestRecommendations, Array.Empty<RestRecommendationDto>())
            .With(x => x.EmomFeasibility, (IReadOnlyList<EmomFeasibilityDto>?)null)
            .With(x => x.CalculatedAt, DateTime.UtcNow)
            .With(x => x.BenchmarkCoverageCount, 2)
            .With(x => x.TotalMovementCount, 2)
            .With(x => x.AveragePercentile, 65m));

        // Customize RestRecommendationDto
        fixture.Customize<RestRecommendationDto>(c => c
            .With(x => x.AfterMovement, "Thruster")
            .With(x => x.MovementDefinitionId, () => fixture.Create<int>())
            .With(x => x.PacingLevel, "Moderate")
            .With(x => x.SuggestedRestSeconds, 10)
            .With(x => x.RestRange, "8-12 seconds")
            .With(x => x.Reasoning, "Average performance - maintain steady output with moderate rest"));

        // Customize EmomFeasibilityDto
        fixture.Customize<EmomFeasibilityDto>(c => c
            .With(x => x.Minute, 1)
            .With(x => x.PrescribedWork, "10 Thrusters")
            .With(x => x.EstimatedCompletionSeconds, 40)
            .With(x => x.IsFeasible, true)
            .With(x => x.BufferSeconds, 20)
            .With(x => x.Recommendation, "On pace - comfortable buffer")
            .With(x => x.MovementNames, new List<string> { "Thruster" }));
    }
}
