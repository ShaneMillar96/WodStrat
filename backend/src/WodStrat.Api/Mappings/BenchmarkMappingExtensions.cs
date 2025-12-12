using WodStrat.Api.ViewModels.Benchmarks;
using WodStrat.Services.Dtos;

namespace WodStrat.Api.Mappings;

/// <summary>
/// Extension methods for mapping between Benchmark API ViewModels and Service DTOs.
/// </summary>
public static class BenchmarkMappingExtensions
{
    /// <summary>
    /// Maps BenchmarkDefinitionDto to BenchmarkDefinitionResponse.
    /// </summary>
    public static BenchmarkDefinitionResponse ToResponse(this BenchmarkDefinitionDto dto)
    {
        return new BenchmarkDefinitionResponse
        {
            Id = dto.Id,
            Name = dto.Name,
            Slug = dto.Slug,
            Description = dto.Description,
            Category = dto.Category,
            MetricType = dto.MetricType,
            Unit = dto.Unit,
            DisplayOrder = dto.DisplayOrder
        };
    }

    /// <summary>
    /// Maps AthleteBenchmarkDto to AthleteBenchmarkResponse.
    /// </summary>
    public static AthleteBenchmarkResponse ToResponse(this AthleteBenchmarkDto dto)
    {
        return new AthleteBenchmarkResponse
        {
            Id = dto.Id,
            AthleteId = dto.AthleteId,
            BenchmarkDefinitionId = dto.BenchmarkDefinitionId,
            BenchmarkName = dto.BenchmarkName,
            BenchmarkCategory = dto.BenchmarkCategory,
            Value = dto.Value,
            FormattedValue = dto.FormattedValue,
            Unit = dto.Unit,
            RecordedAt = dto.RecordedAt,
            Notes = dto.Notes,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }

    /// <summary>
    /// Maps BenchmarkSummaryDto to BenchmarkSummaryResponse.
    /// </summary>
    public static BenchmarkSummaryResponse ToResponse(this BenchmarkSummaryDto dto)
    {
        return new BenchmarkSummaryResponse
        {
            AthleteId = dto.AthleteId,
            TotalBenchmarks = dto.TotalBenchmarks,
            MeetsMinimumRequirement = dto.MeetsMinimumRequirement,
            MinimumRequired = dto.MinimumRequired,
            BenchmarksByCategory = dto.BenchmarksByCategory,
            Benchmarks = dto.Benchmarks.Select(b => b.ToResponse()).ToList()
        };
    }

    /// <summary>
    /// Maps RecordBenchmarkRequest to RecordBenchmarkDto.
    /// </summary>
    public static RecordBenchmarkDto ToDto(this RecordBenchmarkRequest request)
    {
        return new RecordBenchmarkDto
        {
            BenchmarkDefinitionId = request.BenchmarkDefinitionId,
            Value = request.Value,
            RecordedAt = request.RecordedAt,
            Notes = request.Notes?.Trim()
        };
    }

    /// <summary>
    /// Maps UpdateBenchmarkRequest to UpdateBenchmarkDto.
    /// </summary>
    public static UpdateBenchmarkDto ToDto(this UpdateBenchmarkRequest request)
    {
        return new UpdateBenchmarkDto
        {
            Value = request.Value,
            RecordedAt = request.RecordedAt,
            Notes = request.Notes?.Trim()
        };
    }
}
