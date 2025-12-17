using WodStrat.Api.ViewModels.Movements;
using WodStrat.Services.Dtos;

namespace WodStrat.Api.Mappings;

/// <summary>
/// Extension methods for mapping between Movement API ViewModels and Service DTOs.
/// </summary>
public static class MovementMappingExtensions
{
    /// <summary>
    /// Maps MovementDefinitionDto to MovementDefinitionResponse.
    /// </summary>
    public static MovementDefinitionResponse ToResponse(this MovementDefinitionDto dto)
    {
        return new MovementDefinitionResponse
        {
            Id = dto.Id,
            CanonicalName = dto.CanonicalName,
            DisplayName = dto.DisplayName,
            Category = dto.Category,
            Description = dto.Description,
            Aliases = dto.Aliases
        };
    }
}
