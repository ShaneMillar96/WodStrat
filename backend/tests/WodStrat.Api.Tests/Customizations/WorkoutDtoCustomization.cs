using AutoFixture;
using WodStrat.Services.Dtos;

namespace WodStrat.Api.Tests.Customizations;

/// <summary>
/// AutoFixture customization for creating valid WorkoutDto instances.
/// </summary>
public class WorkoutDtoCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<WorkoutDto>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.UserId, () => fixture.Create<int>())
            .With(x => x.Name, "Test Workout")
            .With(x => x.WorkoutType, "ForTime")
            .With(x => x.OriginalText, "21-15-9\nThrusters\nPull-ups")
            .With(x => x.ParsedDescription, "FOR TIME - 2 movement(s)")
            .With(x => x.TimeCapSeconds, 1200)
            .With(x => x.TimeCapFormatted, "20:00")
            .With(x => x.RoundCount, (int?)null)
            .With(x => x.IntervalDurationSeconds, (int?)null)
            .With(x => x.IntervalDurationFormatted, (string?)null)
            .With(x => x.Movements, new List<WorkoutMovementDto>())
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-1))
            .With(x => x.UpdatedAt, DateTime.UtcNow));

        fixture.Customize<WorkoutMovementDto>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.MovementDefinitionId, 1)
            .With(x => x.MovementName, "Thruster")
            .With(x => x.MovementCategory, "Weightlifting")
            .With(x => x.SequenceOrder, 1)
            .With(x => x.RepCount, 21)
            .With(x => x.LoadValue, 95m)
            .With(x => x.LoadUnit, "Lb")
            .With(x => x.LoadFormatted, "95 lb")
            .With(x => x.DistanceValue, (decimal?)null)
            .With(x => x.DistanceUnit, (string?)null)
            .With(x => x.DistanceFormatted, (string?)null)
            .With(x => x.Calories, (int?)null)
            .With(x => x.DurationSeconds, (int?)null)
            .With(x => x.DurationFormatted, (string?)null)
            .With(x => x.Notes, (string?)null));

        fixture.Customize<ParsedWorkoutDto>(c => c
            .With(x => x.OriginalText, "20 min AMRAP\n10 Pull-ups")
            .With(x => x.ParsedDescription, "AMRAP - 20 min - 1 movement(s)")
            .With(x => x.WorkoutType, Dal.Enums.WorkoutType.Amrap)
            .With(x => x.TimeCapSeconds, 1200)
            .With(x => x.RoundCount, (int?)null)
            .With(x => x.IntervalDurationSeconds, (int?)null)
            .With(x => x.Movements, new List<ParsedMovementDto>())
            .With(x => x.Errors, new List<ParsingErrorDto>()));

        fixture.Customize<ParsedMovementDto>(c => c
            .With(x => x.OriginalText, "10 Pull-ups")
            .With(x => x.MovementDefinitionId, 1)
            .With(x => x.MovementName, "Pull-up")
            .With(x => x.SequenceOrder, 1)
            .With(x => x.RepCount, 10)
            .With(x => x.LoadValue, (decimal?)null)
            .With(x => x.LoadUnit, (Dal.Enums.LoadUnit?)null)
            .With(x => x.DistanceValue, (decimal?)null)
            .With(x => x.DistanceUnit, (Dal.Enums.DistanceUnit?)null)
            .With(x => x.Calories, (int?)null)
            .With(x => x.DurationSeconds, (int?)null)
            .With(x => x.Notes, (string?)null));
    }
}
