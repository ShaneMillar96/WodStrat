using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;

namespace WodStrat.Services.Extensions;

/// <summary>
/// Extension methods for mapping between Movement entities and DTOs.
/// </summary>
public static class MovementMappingExtensions
{
    /// <summary>
    /// Maps a MovementDefinition entity to a MovementDefinitionDto.
    /// </summary>
    /// <param name="entity">The movement definition entity.</param>
    /// <returns>The movement definition DTO.</returns>
    public static MovementDefinitionDto ToDto(this MovementDefinition entity)
    {
        return new MovementDefinitionDto
        {
            Id = entity.Id,
            CanonicalName = entity.CanonicalName,
            DisplayName = entity.DisplayName,
            Category = entity.Category.ToString(),
            Description = entity.Description,
            Aliases = entity.Aliases?.Select(a => a.Alias).ToList() ?? new List<string>()
        };
    }
}
