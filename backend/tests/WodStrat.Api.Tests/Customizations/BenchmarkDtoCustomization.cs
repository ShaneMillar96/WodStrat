using AutoFixture;
using WodStrat.Services.Dtos;

namespace WodStrat.Api.Tests.Customizations;

/// <summary>
/// AutoFixture customization for creating valid Benchmark DTOs.
/// </summary>
public class BenchmarkDtoCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        // Register DateOnly generator to avoid AutoFixture issues with DateOnly
        fixture.Register(() => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)));

        fixture.Customize<BenchmarkDefinitionDto>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.Name, "500m Row")
            .With(x => x.Slug, "500m-row")
            .With(x => x.Description, "500 meter row for time")
            .With(x => x.Category, "Cardio")
            .With(x => x.MetricType, "Time")
            .With(x => x.Unit, "seconds")
            .With(x => x.DisplayOrder, 1));

        fixture.Customize<AthleteBenchmarkDto>(c => c
            .With(x => x.Id, () => fixture.Create<int>())
            .With(x => x.AthleteId, () => fixture.Create<int>())
            .With(x => x.BenchmarkDefinitionId, () => fixture.Create<int>())
            .With(x => x.BenchmarkName, "500m Row")
            .With(x => x.BenchmarkSlug, "500m-row")
            .With(x => x.BenchmarkCategory, "Cardio")
            .With(x => x.MetricType, "Time")
            .With(x => x.Unit, "seconds")
            .With(x => x.Value, 195.5m)
            .With(x => x.FormattedValue, "3:15")
            .With(x => x.RecordedAt, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)))
            .With(x => x.Notes, (string?)null)
            .With(x => x.CreatedAt, DateTime.UtcNow.AddDays(-7))
            .With(x => x.UpdatedAt, DateTime.UtcNow.AddDays(-1)));

        fixture.Customize<BenchmarkSummaryDto>(c => c
            .With(x => x.AthleteId, () => fixture.Create<int>())
            .With(x => x.TotalBenchmarks, 5)
            .With(x => x.MeetsMinimumRequirement, true)
            .With(x => x.MinimumRequired, 3)
            .With(x => x.BenchmarksByCategory, new Dictionary<string, int> { { "Cardio", 3 }, { "Strength", 2 } })
            .With(x => x.Benchmarks, new List<AthleteBenchmarkDto>()));
    }
}
