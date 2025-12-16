using WodStrat.Dal.Models;
using WodStrat.Services.Dtos.Auth;

namespace WodStrat.Services.Extensions;

/// <summary>
/// Extension methods for mapping between User entities and DTOs.
/// </summary>
public static class AuthMappingExtensions
{
    /// <summary>
    /// Maps a RegisterDto to a new User entity.
    /// Password hashing is handled in AuthService, not here.
    /// </summary>
    /// <param name="dto">The registration DTO.</param>
    /// <param name="passwordHash">Pre-computed BCrypt hash.</param>
    /// <returns>A new User entity.</returns>
    public static User ToEntity(this RegisterDto dto, string passwordHash)
    {
        var now = DateTime.UtcNow;
        return new User
        {
            Email = dto.Email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Maps a User entity to UserDto.
    /// </summary>
    /// <param name="entity">The user entity.</param>
    /// <returns>User DTO without sensitive data.</returns>
    public static UserDto ToDto(this User entity)
    {
        return new UserDto
        {
            Id = entity.Id,
            Email = entity.Email,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
