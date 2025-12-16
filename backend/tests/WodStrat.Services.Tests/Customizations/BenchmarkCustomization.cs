using AutoFixture;
using AutoFixture.Kernel;
using WodStrat.Dal.Enums;
using WodStrat.Dal.Models;

namespace WodStrat.Services.Tests.Customizations;

/// <summary>
/// AutoFixture customization for creating valid BenchmarkDefinition and AthleteBenchmark entities.
/// </summary>
public class BenchmarkCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        // Replace ThrowingRecursionBehavior with OmitOnRecursionBehavior to handle circular references
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // Register DateOnly generator to avoid AutoFixture issues
        fixture.Register(() => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)));

        // Customize User to avoid circular references (must be before Athlete)
        fixture.Customize<User>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.Email, "test@example.com")
            .With(x => x.PasswordHash, "$2a$11$K6xU/A3R.ZgM0KxWR/KZIeM/Z0z7xbsS8v4MZ.4D1G4D1G4D1G4D1")
            .With(x => x.IsActive, true)
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-7))
            .With(x => x.UpdatedAt, DateTime.UtcNow.AddDays(-1))
            .Without(x => x.Athlete)); // Avoid circular reference with Athlete

        // Customize Athlete to avoid circular references with Benchmarks navigation
        fixture.Customize<Athlete>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.Name, "Test Athlete")
            .With(x => x.DateOfBirth, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)))
            .With(x => x.Gender, "Male")
            .With(x => x.HeightCm, 175m)
            .With(x => x.WeightKg, 80m)
            .With(x => x.ExperienceLevel, ExperienceLevel.Intermediate)
            .With(x => x.PrimaryGoal, AthleteGoal.ImprovePacing)
            .With(x => x.IsDeleted, false)
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-7))
            .With(x => x.UpdatedAt, DateTime.UtcNow.AddDays(-1))
            .Without(x => x.Benchmarks) // Avoid circular reference
            .Without(x => x.User)); // Avoid circular reference with User

        // Customize BenchmarkDefinition to avoid circular references
        fixture.Customize<BenchmarkDefinition>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.Name, "500m Row")
            .With(x => x.Slug, "500m-row")
            .With(x => x.Description, "500 meter row for time")
            .With(x => x.Category, BenchmarkCategory.Cardio)
            .With(x => x.MetricType, BenchmarkMetricType.Time)
            .With(x => x.Unit, "seconds")
            .With(x => x.IsActive, true)
            .With(x => x.DisplayOrder, 1)
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-30))
            .Without(x => x.AthleteBenchmarks)); // Avoid circular reference

        // Customize AthleteBenchmark to avoid circular references
        fixture.Customize<AthleteBenchmark>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.AthleteId, () => fixture.Create<int>())
            .With(x => x.BenchmarkDefinitionId, () => fixture.Create<int>())
            .With(x => x.Value, 195.5m) // 3:15.5 in seconds
            .With(x => x.RecordedAt, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)))
            .With(x => x.Notes, (string?)null)
            .With(x => x.IsDeleted, false)
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-7))
            .With(x => x.UpdatedAt, DateTime.UtcNow.AddDays(-1))
            .Without(x => x.Athlete) // Avoid circular reference
            .Without(x => x.BenchmarkDefinition)); // Set manually when needed
    }
}
