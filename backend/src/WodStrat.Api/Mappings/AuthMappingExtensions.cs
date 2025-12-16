using WodStrat.Api.ViewModels.Auth;
using WodStrat.Services.Dtos.Auth;

namespace WodStrat.Api.Mappings;

/// <summary>
/// Extension methods for mapping between Auth API ViewModels and Service DTOs.
/// </summary>
public static class AuthMappingExtensions
{
    /// <summary>
    /// Maps RegisterRequest to RegisterDto.
    /// </summary>
    public static RegisterDto ToDto(this RegisterRequest request)
    {
        return new RegisterDto
        {
            Email = request.Email.Trim().ToLowerInvariant(),
            Password = request.Password,
            ConfirmPassword = request.ConfirmPassword
        };
    }

    /// <summary>
    /// Maps LoginRequest to LoginDto.
    /// </summary>
    public static LoginDto ToDto(this LoginRequest request)
    {
        return new LoginDto
        {
            Email = request.Email.Trim().ToLowerInvariant(),
            Password = request.Password
        };
    }

    /// <summary>
    /// Maps AuthResponseDto to AuthResponse.
    /// </summary>
    public static AuthResponse ToResponse(this AuthResponseDto dto)
    {
        return new AuthResponse
        {
            Token = dto.Token,
            TokenType = dto.TokenType,
            ExpiresIn = dto.ExpiresIn,
            Email = dto.Email,
            UserId = dto.UserId,
            HasAthleteProfile = dto.HasAthleteProfile,
            AthleteId = dto.AthleteId
        };
    }
}
