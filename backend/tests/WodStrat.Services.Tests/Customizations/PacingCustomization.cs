using AutoFixture;
using WodStrat.Dal.Enums;
using WodStrat.Dal.Models;

namespace WodStrat.Services.Tests.Customizations;

/// <summary>
/// AutoFixture customization for creating valid pacing-related entities.
/// </summary>
public class PacingCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        // Replace ThrowingRecursionBehavior with OmitOnRecursionBehavior to handle circular references
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // Register DateOnly generator
        fixture.Register(() => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)));

        // Customize User
        fixture.Customize<User>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.Email, "test@example.com")
            .With(x => x.PasswordHash, "$2a$11$K6xU/A3R.ZgM0KxWR/KZIeM/Z0z7xbsS8v4MZ.4D1G4D1G4D1G4D1")
            .With(x => x.IsActive, true)
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-7))
            .With(x => x.UpdatedAt, DateTime.UtcNow.AddDays(-1))
            .Without(x => x.Athlete));

        // Customize Athlete
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
            .Without(x => x.Benchmarks)
            .Without(x => x.User));

        // Customize BenchmarkDefinition
        fixture.Customize<BenchmarkDefinition>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.Name, "Back Squat 1RM")
            .With(x => x.Slug, "back-squat-1rm")
            .With(x => x.Description, "Back squat one rep max")
            .With(x => x.Category, BenchmarkCategory.Strength)
            .With(x => x.MetricType, BenchmarkMetricType.Weight)
            .With(x => x.Unit, "kg")
            .With(x => x.IsActive, true)
            .With(x => x.DisplayOrder, 1)
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-30))
            .Without(x => x.AthleteBenchmarks)
            .Without(x => x.MovementMappings)
            .Without(x => x.PopulationPercentiles));

        // Customize AthleteBenchmark
        fixture.Customize<AthleteBenchmark>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.AthleteId, () => fixture.Create<int>())
            .With(x => x.BenchmarkDefinitionId, () => fixture.Create<int>())
            .With(x => x.Value, 120m)
            .With(x => x.RecordedAt, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)))
            .With(x => x.Notes, (string?)null)
            .With(x => x.IsDeleted, false)
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-7))
            .With(x => x.UpdatedAt, DateTime.UtcNow.AddDays(-1))
            .Without(x => x.Athlete)
            .Without(x => x.BenchmarkDefinition));

        // Customize MovementDefinition
        fixture.Customize<MovementDefinition>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.CanonicalName, "thruster")
            .With(x => x.DisplayName, "Thruster")
            .With(x => x.Category, MovementCategory.Weightlifting)
            .With(x => x.IsActive, true)
            .With(x => x.IsDeleted, false)
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-30))
            .With(x => x.UpdatedAt, DateTime.UtcNow.AddDays(-1))
            .Without(x => x.Aliases)
            .Without(x => x.BenchmarkMappings));

        // Customize BenchmarkMovementMapping
        fixture.Customize<BenchmarkMovementMapping>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.BenchmarkDefinitionId, () => fixture.Create<int>())
            .With(x => x.MovementDefinitionId, () => fixture.Create<int>())
            .With(x => x.RelevanceFactor, 1.0m)
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-30))
            .Without(x => x.BenchmarkDefinition)
            .Without(x => x.MovementDefinition));

        // Customize PopulationBenchmarkPercentile
        fixture.Customize<PopulationBenchmarkPercentile>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.BenchmarkDefinitionId, () => fixture.Create<int>())
            .With(x => x.Percentile20, 70m)
            .With(x => x.Percentile40, 90m)
            .With(x => x.Percentile60, 110m)
            .With(x => x.Percentile80, 135m)
            .With(x => x.Percentile95, 170m)
            .With(x => x.Gender, "Male")
            .With(x => x.ExperienceLevel, (ExperienceLevel?)null)
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-30))
            .With(x => x.UpdatedAt, DateTime.UtcNow.AddDays(-1))
            .Without(x => x.BenchmarkDefinition));

        // Customize Workout
        fixture.Customize<Workout>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.Name, "Test Workout")
            .With(x => x.ParsedDescription, "A test workout")
            .With(x => x.WorkoutType, WorkoutType.ForTime)
            .With(x => x.RepSchemeType, RepSchemeType.Fixed)
            .With(x => x.UserId, () => fixture.Create<int>())
            .With(x => x.IsDeleted, false)
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-7))
            .With(x => x.UpdatedAt, DateTime.UtcNow.AddDays(-1))
            .Without(x => x.User)
            .Without(x => x.Movements));

        // Customize WorkoutMovement
        fixture.Customize<WorkoutMovement>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.WorkoutId, () => fixture.Create<int>())
            .With(x => x.MovementDefinitionId, () => fixture.Create<int>())
            .With(x => x.SequenceOrder, 1)
            .With(x => x.RepCount, 21)
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-7))
            .Without(x => x.Workout)
            .Without(x => x.MovementDefinition));
    }
}
