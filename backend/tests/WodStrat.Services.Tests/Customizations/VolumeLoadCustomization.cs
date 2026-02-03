using AutoFixture;
using WodStrat.Dal.Enums;
using WodStrat.Dal.Models;

namespace WodStrat.Services.Tests.Customizations;

/// <summary>
/// AutoFixture customization for creating valid volume load-related entities.
/// Extends PacingCustomization with additional volume load specific setup.
/// </summary>
public class VolumeLoadCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        // Apply the pacing customization first (shares many entities)
        new PacingCustomization().Customize(fixture);

        // Override MovementDefinition to include IsBodyweight
        fixture.Customize<MovementDefinition>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.CanonicalName, "thruster")
            .With(x => x.DisplayName, "Thruster")
            .With(x => x.Category, MovementCategory.Weightlifting)
            .With(x => x.IsBodyweight, false)
            .With(x => x.IsActive, true)
            .With(x => x.IsDeleted, false)
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-30))
            .With(x => x.UpdatedAt, DateTime.UtcNow.AddDays(-1))
            .Without(x => x.Aliases)
            .Without(x => x.BenchmarkMappings));

        // Customize WorkoutMovement with load values for volume calculation
        fixture.Customize<WorkoutMovement>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.WorkoutId, () => fixture.Create<int>())
            .With(x => x.MovementDefinitionId, () => fixture.Create<int>())
            .With(x => x.SequenceOrder, 1)
            .With(x => x.RepCount, 21)
            .With(x => x.LoadValue, 43m) // Default to RX Thruster weight
            .With(x => x.LoadUnit, LoadUnit.Kg)
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-7))
            .Without(x => x.Workout)
            .Without(x => x.MovementDefinition));

        // Customize Workout with round count
        fixture.Customize<Workout>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.Name, "Test Workout")
            .With(x => x.ParsedDescription, "A test workout")
            .With(x => x.WorkoutType, WorkoutType.ForTime)
            .With(x => x.RepSchemeType, RepSchemeType.Fixed)
            .With(x => x.RoundCount, 1) // Default single round
            .With(x => x.UserId, () => fixture.Create<int>())
            .With(x => x.IsDeleted, false)
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-7))
            .With(x => x.UpdatedAt, DateTime.UtcNow.AddDays(-1))
            .Without(x => x.User)
            .Without(x => x.Movements));
    }
}
