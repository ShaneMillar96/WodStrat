using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;
using WodStrat.Services.Utilities;

namespace WodStrat.Services.Extensions;

/// <summary>
/// Extension methods for mapping between Benchmark entities and DTOs.
/// </summary>
public static class BenchmarkMappingExtensions
{
    /// <summary>
    /// Maps a BenchmarkDefinition entity to a BenchmarkDefinitionDto.
    /// </summary>
    /// <param name="entity">The benchmark definition entity.</param>
    /// <returns>The benchmark definition DTO.</returns>
    public static BenchmarkDefinitionDto ToDto(this BenchmarkDefinition entity)
    {
        return new BenchmarkDefinitionDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Slug = entity.Slug,
            Description = entity.Description,
            Category = entity.Category.ToString(),
            MetricType = entity.MetricType.ToString(),
            Unit = entity.Unit,
            DisplayOrder = entity.DisplayOrder
        };
    }

    /// <summary>
    /// Maps an AthleteBenchmark entity to an AthleteBenchmarkDto.
    /// Requires BenchmarkDefinition navigation property to be loaded.
    /// </summary>
    /// <param name="entity">The athlete benchmark entity (must include BenchmarkDefinition navigation).</param>
    /// <returns>The athlete benchmark DTO with formatted value.</returns>
    public static AthleteBenchmarkDto ToDto(this AthleteBenchmark entity)
    {
        var definition = entity.BenchmarkDefinition;

        return new AthleteBenchmarkDto
        {
            Id = entity.Id,
            AthleteId = entity.AthleteId,
            BenchmarkDefinitionId = entity.BenchmarkDefinitionId,
            BenchmarkName = definition?.Name ?? string.Empty,
            BenchmarkSlug = definition?.Slug ?? string.Empty,
            BenchmarkCategory = definition?.Category.ToString() ?? string.Empty,
            MetricType = definition?.MetricType.ToString() ?? string.Empty,
            Unit = definition?.Unit ?? string.Empty,
            Value = entity.Value,
            FormattedValue = BenchmarkValueFormatter.Format(
                entity.Value,
                definition?.MetricType.ToString() ?? "Reps",
                definition?.Unit ?? string.Empty),
            RecordedAt = entity.RecordedAt,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    /// <summary>
    /// Maps a RecordBenchmarkDto to a new AthleteBenchmark entity.
    /// </summary>
    /// <param name="dto">The record DTO.</param>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <returns>A new athlete benchmark entity.</returns>
    public static AthleteBenchmark ToEntity(this RecordBenchmarkDto dto, Guid athleteId)
    {
        return new AthleteBenchmark
        {
            Id = Guid.NewGuid(),
            AthleteId = athleteId,
            BenchmarkDefinitionId = dto.BenchmarkDefinitionId,
            Value = dto.Value,
            RecordedAt = dto.RecordedAt ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Notes = dto.Notes,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Applies values from UpdateBenchmarkDto to an existing AthleteBenchmark entity.
    /// </summary>
    /// <param name="dto">The update DTO.</param>
    /// <param name="entity">The entity to update.</param>
    public static void ApplyTo(this UpdateBenchmarkDto dto, AthleteBenchmark entity)
    {
        entity.Value = dto.Value;

        if (dto.RecordedAt.HasValue)
        {
            entity.RecordedAt = dto.RecordedAt.Value;
        }

        // Allow clearing notes by passing empty string or null
        entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
