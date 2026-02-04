using AutoFixture;
using WodStrat.Dal.Enums;
using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;

namespace WodStrat.Services.Tests.Customizations;

/// <summary>
/// AutoFixture customization for creating valid time estimate-related entities.
/// </summary>
public class TimeEstimateCustomization : ICustomization
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
            .Without(x => x.Athlete)
            .Without(x => x.Workouts));

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
            .Without(x => x.BenchmarkMappings)
            .Without(x => x.WorkoutMovements));

        // Customize Workout
        fixture.Customize<Workout>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.Name, "Test Workout")
            .With(x => x.ParsedDescription, "A test workout")
            .With(x => x.WorkoutType, WorkoutType.ForTime)
            .With(x => x.RepSchemeType, RepSchemeType.Fixed)
            .With(x => x.UserId, () => fixture.Create<int>())
            .With(x => x.TimeCapSeconds, 1200)
            .With(x => x.RoundCount, 1)
            .With(x => x.IntervalDurationSeconds, (int?)null)
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
            .With(x => x.LoadValue, 43m)
            .With(x => x.LoadUnit, LoadUnit.Kg)
            .With(x => x.DistanceValue, (decimal?)null)
            .With(x => x.DistanceUnit, (DistanceUnit?)null)
            .With(x => x.Calories, (int?)null)
            .With(x => x.DurationSeconds, (int?)null)
            .With(x => x.MinuteStart, (int?)null)
            .With(x => x.MinuteEnd, (int?)null)
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-7))
            .Without(x => x.Workout)
            .Without(x => x.MovementDefinition));

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

        // Customize MovementPacingDto
        fixture.Customize<MovementPacingDto>(c => c
            .With(x => x.MovementDefinitionId, () => fixture.Create<int>())
            .With(x => x.MovementName, "Thruster")
            .With(x => x.PacingLevel, "Moderate")
            .With(x => x.AthletePercentile, 65m)
            .With(x => x.GuidanceText, "Steady pace through reps")
            .With(x => x.RecommendedSets, Array.Empty<int>())
            .With(x => x.BenchmarkUsed, "Clean & Jerk 1RM"));

        // Customize WorkoutTimeReference
        fixture.Customize<WorkoutTimeReference>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.WorkoutName, "Fran")
            .With(x => x.Gender, "Male")
            .With(x => x.ExperienceLevel, (ExperienceLevel?)null)
            .With(x => x.Percentile20Seconds, 420)
            .With(x => x.Percentile40Seconds, 330)
            .With(x => x.Percentile60Seconds, 270)
            .With(x => x.Percentile80Seconds, 210)
            .With(x => x.Percentile95Seconds, 150)
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-30))
            .With(x => x.UpdatedAt, DateTime.UtcNow.AddDays(-1)));
    }
}
