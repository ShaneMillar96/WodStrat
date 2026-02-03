using AutoFixture;
using WodStrat.Services.Dtos;

namespace WodStrat.Api.Tests.Customizations;

/// <summary>
/// AutoFixture customization for creating valid pacing-related DTOs.
/// </summary>
public class PacingDtoCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        // Replace ThrowingRecursionBehavior with OmitOnRecursionBehavior
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // Customize MovementPacingDto
        fixture.Customize<MovementPacingDto>(c => c
            .With(x => x.MovementDefinitionId, () => fixture.Create<int>())
            .With(x => x.MovementName, "Thruster")
            .With(x => x.PacingLevel, "Heavy")
            .With(x => x.AthletePercentile, 85m)
            .With(x => x.GuidanceText, "Push hard on Thrusters. Aim for large sets (13-8) or go unbroken if possible.")
            .With(x => x.RecommendedSets, new[] { 13, 8 })
            .With(x => x.BenchmarkUsed, "Front Squat 1RM")
            .With(x => x.HasPopulationData, true)
            .With(x => x.HasAthleteBenchmark, true));

        // Customize PacingDistributionDto
        fixture.Customize<PacingDistributionDto>(c => c
            .With(x => x.HeavyCount, 2)
            .With(x => x.ModerateCount, 1)
            .With(x => x.LightCount, 0)
            .With(x => x.TotalMovements, 3)
            .With(x => x.IncompleteDataCount, 0));

        // Customize WorkoutPacingResultDto
        fixture.Customize<WorkoutPacingResultDto>(c => c
            .With(x => x.WorkoutId, () => fixture.Create<int>())
            .With(x => x.WorkoutName, "Fran")
            .With(x => x.WorkoutType, "ForTime")
            .With(x => x.MovementPacing, () => fixture.CreateMany<MovementPacingDto>(2).ToList())
            .With(x => x.OverallStrategyNotes, "This is a sprint workout. Push hard on all movements.")
            .With(x => x.CalculatedAt, DateTime.UtcNow)
            .With(x => x.Distribution, () => fixture.Create<PacingDistributionDto>())
            .With(x => x.IsComplete, true));

        // Customize WorkoutPacingRequestDto
        fixture.Customize<WorkoutPacingRequestDto>(c => c
            .With(x => x.AthleteId, () => fixture.Create<int>())
            .With(x => x.WorkoutId, () => fixture.Create<int>()));
    }
}
