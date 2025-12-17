using AutoFixture;
using WodStrat.Dal.Enums;
using WodStrat.Dal.Models;

namespace WodStrat.Services.Tests.Customizations;

/// <summary>
/// AutoFixture customization for creating valid Workout-related entities.
/// </summary>
public class WorkoutCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        // Replace ThrowingRecursionBehavior with OmitOnRecursionBehavior to handle circular references
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // Customize User to avoid circular references
        fixture.Customize<User>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.Email, "test@example.com")
            .With(x => x.PasswordHash, "$2a$11$K6xU/A3R.ZgM0KxWR/KZIeM/Z0z7xbsS8v4MZ.4D1G4D1G4D1G4D1")
            .With(x => x.IsActive, true)
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-7))
            .With(x => x.UpdatedAt, DateTime.UtcNow.AddDays(-1))
            .Without(x => x.Athlete)
            .Without(x => x.Workouts));

        // Customize MovementDefinition
        fixture.Customize<MovementDefinition>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.CanonicalName, "pull_up")
            .With(x => x.DisplayName, "Pull-up")
            .With(x => x.Category, MovementCategory.Gymnastics)
            .With(x => x.Description, "Vertical pulling movement")
            .With(x => x.IsActive, true)
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-30))
            .Without(x => x.Aliases)
            .Without(x => x.WorkoutMovements));

        // Customize MovementAlias
        fixture.Customize<MovementAlias>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.Alias, "pu")
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-30))
            .Without(x => x.MovementDefinition));

        // Customize Workout
        fixture.Customize<Workout>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.UserId, () => fixture.Create<int>())
            .With(x => x.Name, "Test Workout")
            .With(x => x.WorkoutType, WorkoutType.ForTime)
            .With(x => x.OriginalText, "21-15-9\nThrusters\nPull-ups")
            .With(x => x.ParsedDescription, "FORTIME - 2 movement(s)")
            .With(x => x.TimeCapSeconds, 1200) // 20 minutes
            .With(x => x.RoundCount, (int?)null)
            .With(x => x.IntervalDurationSeconds, (int?)null)
            .With(x => x.IsDeleted, false)
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-1))
            .With(x => x.UpdatedAt, DateTime.UtcNow)
            .Without(x => x.User)
            .Without(x => x.Movements));

        // Customize WorkoutMovement
        fixture.Customize<WorkoutMovement>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.SequenceOrder, 1)
            .With(x => x.RepCount, 21)
            .With(x => x.LoadValue, 95m)
            .With(x => x.LoadUnit, LoadUnit.Lb)
            .With(x => x.DistanceValue, (decimal?)null)
            .With(x => x.DistanceUnit, (DistanceUnit?)null)
            .With(x => x.Calories, (int?)null)
            .With(x => x.DurationSeconds, (int?)null)
            .With(x => x.Notes, (string?)null)
            .With(x => x.CreatedAt, DateTime.UtcNow)
            .Without(x => x.Workout)
            .Without(x => x.MovementDefinition));
    }
}
