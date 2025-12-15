using WodStrat.Dal.Enums;
using WodStrat.Dal.Models;
using WodStrat.Services.Dtos;

namespace WodStrat.Services.Extensions;

/// <summary>
/// Extension methods for mapping between Athlete entities and DTOs.
/// </summary>
public static class AthleteMappingExtensions
{
    /// <summary>
    /// Maps an Athlete entity to an AthleteDto.
    /// </summary>
    /// <param name="entity">The athlete entity.</param>
    /// <returns>The athlete DTO with calculated age.</returns>
    public static AthleteDto ToDto(this Athlete entity)
    {
        return new AthleteDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Age = CalculateAge(entity.DateOfBirth),
            Gender = entity.Gender,
            HeightCm = entity.HeightCm,
            WeightKg = entity.WeightKg,
            ExperienceLevel = entity.ExperienceLevel.ToString(),
            PrimaryGoal = entity.PrimaryGoal.ToString(),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    /// <summary>
    /// Maps a CreateAthleteDto to a new Athlete entity.
    /// </summary>
    /// <param name="dto">The create DTO.</param>
    /// <returns>A new athlete entity.</returns>
    public static Athlete ToEntity(this CreateAthleteDto dto)
    {
        var entity = new Athlete
        {
            // Id is NOT set - database will auto-generate via SERIAL/IDENTITY
            Name = dto.Name,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            HeightCm = dto.HeightCm,
            WeightKg = dto.WeightKg,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Parse experience level if provided, otherwise use default
        if (!string.IsNullOrWhiteSpace(dto.ExperienceLevel) &&
            Enum.TryParse<ExperienceLevel>(dto.ExperienceLevel, ignoreCase: true, out var experienceLevel))
        {
            entity.ExperienceLevel = experienceLevel;
        }

        // Parse primary goal if provided, otherwise use default
        if (!string.IsNullOrWhiteSpace(dto.PrimaryGoal) &&
            Enum.TryParse<AthleteGoal>(dto.PrimaryGoal, ignoreCase: true, out var primaryGoal))
        {
            entity.PrimaryGoal = primaryGoal;
        }

        return entity;
    }

    /// <summary>
    /// Applies values from UpdateAthleteDto to an existing Athlete entity.
    /// </summary>
    /// <param name="dto">The update DTO.</param>
    /// <param name="entity">The entity to update.</param>
    public static void ApplyTo(this UpdateAthleteDto dto, Athlete entity)
    {
        entity.Name = dto.Name;
        entity.DateOfBirth = dto.DateOfBirth;
        entity.Gender = dto.Gender;
        entity.HeightCm = dto.HeightCm;
        entity.WeightKg = dto.WeightKg;

        if (!string.IsNullOrWhiteSpace(dto.ExperienceLevel) &&
            Enum.TryParse<ExperienceLevel>(dto.ExperienceLevel, ignoreCase: true, out var experienceLevel))
        {
            entity.ExperienceLevel = experienceLevel;
        }

        if (!string.IsNullOrWhiteSpace(dto.PrimaryGoal) &&
            Enum.TryParse<AthleteGoal>(dto.PrimaryGoal, ignoreCase: true, out var primaryGoal))
        {
            entity.PrimaryGoal = primaryGoal;
        }

        entity.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calculates age from date of birth.
    /// </summary>
    /// <param name="dateOfBirth">The date of birth.</param>
    /// <returns>The calculated age, or null if date of birth is null.</returns>
    public static int? CalculateAge(DateOnly? dateOfBirth)
    {
        if (!dateOfBirth.HasValue)
            return null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dateOfBirth.Value.Year;

        // Adjust if birthday hasn't occurred yet this year
        if (dateOfBirth.Value > today.AddYears(-age))
            age--;

        return age;
    }
}
