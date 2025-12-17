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

    /// <summary>
    /// Maps a MovementAlias entity to a MovementAliasDto.
    /// </summary>
    /// <param name="entity">The movement alias entity.</param>
    /// <returns>The movement alias DTO.</returns>
    public static MovementAliasDto ToDto(this MovementAlias entity)
    {
        return new MovementAliasDto
        {
            Id = entity.Id,
            MovementDefinitionId = entity.MovementDefinitionId,
            Alias = entity.Alias,
            CreatedAt = entity.CreatedAt
        };
    }

    /// <summary>
    /// Creates a NormalizeMovementResultDto from a normalization result.
    /// </summary>
    /// <param name="input">The original input string.</param>
    /// <param name="movement">The matched movement definition, or null if no match.</param>
    /// <returns>The normalization result DTO.</returns>
    public static NormalizeMovementResultDto ToNormalizeResult(string input, MovementDefinitionDto? movement)
    {
        return new NormalizeMovementResultDto
        {
            Input = input,
            CanonicalName = movement?.CanonicalName,
            IsMatch = movement != null,
            Movement = movement
        };
    }
}
