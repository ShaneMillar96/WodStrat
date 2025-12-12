using AutoFixture;
using WodStrat.Dal.Enums;
using WodStrat.Dal.Models;

namespace WodStrat.Services.Tests.Customizations;

/// <summary>
/// AutoFixture customization for creating valid Athlete entities.
/// </summary>
public class AthleteCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        // Register DateOnly generator to avoid AutoFixture issues with DateOnly
        fixture.Register(() => DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)));

        fixture.Customize<Athlete>(c => c
            .With(x => x.Id, Guid.NewGuid())
            .With(x => x.Name, "Test Athlete")
            .With(x => x.DateOfBirth, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)))
            .With(x => x.Gender, "Male")
            .With(x => x.HeightCm, 175m)
            .With(x => x.WeightKg, 80m)
            .With(x => x.ExperienceLevel, ExperienceLevel.Intermediate)
            .With(x => x.PrimaryGoal, AthleteGoal.ImprovePacing)
            .With(x => x.IsDeleted, false)
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-7))
            .With(x => x.UpdatedAt, DateTime.UtcNow.AddDays(-1)));
    }
}
