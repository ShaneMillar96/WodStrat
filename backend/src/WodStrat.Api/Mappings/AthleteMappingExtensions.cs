using WodStrat.Api.ViewModels.Athletes;
using WodStrat.Services.Dtos;

namespace WodStrat.Api.Mappings;

/// <summary>
/// Extension methods for mapping between API ViewModels and Service DTOs.
/// </summary>
public static class AthleteMappingExtensions
{
    /// <summary>
    /// Maps AthleteDto to AthleteResponse.
    /// </summary>
    public static AthleteResponse ToResponse(this AthleteDto dto)
    {
        return new AthleteResponse
        {
            Id = dto.Id,
            Name = dto.Name,
            Age = dto.Age,
            Gender = dto.Gender,
            HeightCm = dto.HeightCm,
            WeightKg = dto.WeightKg,
            ExperienceLevel = dto.ExperienceLevel,
            PrimaryGoal = dto.PrimaryGoal,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }

    /// <summary>
    /// Maps CreateAthleteRequest to CreateAthleteDto.
    /// </summary>
    public static CreateAthleteDto ToDto(this CreateAthleteRequest request)
    {
        return new CreateAthleteDto
        {
            Name = request.Name.Trim(),
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            HeightCm = request.HeightCm,
            WeightKg = request.WeightKg,
            ExperienceLevel = request.ExperienceLevel,
            PrimaryGoal = request.PrimaryGoal
        };
    }

    /// <summary>
    /// Maps UpdateAthleteRequest to UpdateAthleteDto.
    /// </summary>
    public static UpdateAthleteDto ToDto(this UpdateAthleteRequest request)
    {
        return new UpdateAthleteDto
        {
            Name = request.Name.Trim(),
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            HeightCm = request.HeightCm,
            WeightKg = request.WeightKg,
            ExperienceLevel = request.ExperienceLevel,
            PrimaryGoal = request.PrimaryGoal
        };
    }
}
