using AutoFixture;
using WodStrat.Services.Dtos;

namespace WodStrat.Api.Tests.Customizations;

/// <summary>
/// AutoFixture customization for creating valid AthleteDto instances.
/// </summary>
public class AthleteDtoCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<AthleteDto>(c => c
            .With(x => x.Id, Guid.NewGuid())
            .With(x => x.Name, "Test Athlete")
            .With(x => x.Age, 25)
            .With(x => x.Gender, "Male")
            .With(x => x.HeightCm, 175m)
            .With(x => x.WeightKg, 80m)
            .With(x => x.ExperienceLevel, "Intermediate")
            .With(x => x.PrimaryGoal, "ImprovePacing")
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-7))
            .With(x => x.UpdatedAt, DateTime.UtcNow.AddDays(-1)));
    }
}
