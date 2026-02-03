using AutoFixture;
using WodStrat.Services.Dtos;

namespace WodStrat.Api.Tests.Customizations;

/// <summary>
/// AutoFixture customization for creating valid volume load-related DTOs.
/// </summary>
public class VolumeLoadDtoCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        // Replace ThrowingRecursionBehavior with OmitOnRecursionBehavior
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // Customize MovementVolumeLoadDto
        fixture.Customize<MovementVolumeLoadDto>(c => c
            .With(x => x.MovementDefinitionId, () => fixture.Create<int>())
            .With(x => x.MovementName, "Thruster")
            .With(x => x.Weight, 43m)
            .With(x => x.WeightUnit, "kg")
            .With(x => x.Reps, 45)
            .With(x => x.Rounds, 1)
            .With(x => x.VolumeLoad, 1935m)
            .With(x => x.VolumeLoadFormatted, "1,935 kg")
            .With(x => x.LoadClassification, "Moderate")
            .With(x => x.BenchmarkUsed, "Back Squat 1RM")
            .With(x => x.AthleteBenchmarkPercentile, 65m)
            .With(x => x.Tip, "Weight is moderate relative to your strength. Focus on consistent pacing.")
            .With(x => x.RecommendedWeight, (decimal?)null)
            .With(x => x.RecommendedWeightFormatted, (string?)null)
            .With(x => x.HasSufficientData, true));

        // Customize VolumeLoadDistributionDto
        fixture.Customize<VolumeLoadDistributionDto>(c => c
            .With(x => x.HighCount, 1)
            .With(x => x.ModerateCount, 1)
            .With(x => x.LowCount, 0)
            .With(x => x.BodyweightCount, 1)
            .With(x => x.TotalMovements, 3)
            .With(x => x.InsufficientDataCount, 0));

        // Customize WorkoutVolumeLoadResultDto
        fixture.Customize<WorkoutVolumeLoadResultDto>(c => c
            .With(x => x.WorkoutId, () => fixture.Create<int>())
            .With(x => x.WorkoutName, "Fran")
            .With(x => x.MovementVolumes, () => fixture.CreateMany<MovementVolumeLoadDto>(2).ToList())
            .With(x => x.TotalVolumeLoad, 1935m)
            .With(x => x.TotalVolumeLoadFormatted, "1,935 kg")
            .With(x => x.OverallAssessment, "This workout has moderate volume load overall. Pace accordingly.")
            .With(x => x.CalculatedAt, DateTime.UtcNow)
            .With(x => x.Distribution, () => fixture.Create<VolumeLoadDistributionDto>()));

        // Customize WorkoutVolumeLoadRequestDto
        fixture.Customize<WorkoutVolumeLoadRequestDto>(c => c
            .With(x => x.AthleteId, () => fixture.Create<int>())
            .With(x => x.WorkoutId, () => fixture.Create<int>()));
    }
}
