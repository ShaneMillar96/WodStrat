using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WodStrat.Api.Controllers;
using WodStrat.Api.ViewModels.Auth;
using WodStrat.Services.Dtos.Auth;
using WodStrat.Services.Interfaces;
using Xunit;

namespace WodStrat.Api.Tests.Controllers;

/// <summary>
/// Unit tests for AuthController.
/// </summary>
public class AuthControllerTests
{
    private readonly IFixture _fixture;
    private readonly IAuthService _authService;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization());

        _authService = Substitute.For<IAuthService>();
        _sut = new AuthController(_authService);
    }

    #region Register Tests

    [Fact]
    public async Task Register_ValidRequest_Returns201WithAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "ValidPassword123!",
            ConfirmPassword = "ValidPassword123!"
        };

        var authResponseDto = new AuthResponseDto
        {
            Token = "jwt-token-here",
            TokenType = "Bearer",
            ExpiresIn = 86400,
            UserId = 1,
            Email = "newuser@example.com",
            HasAthleteProfile = false,
            AthleteId = null
        };

        _authService.RegisterAsync(Arg.Any<RegisterDto>(), Arg.Any<CancellationToken>())
            .Returns(AuthResult<AuthResponseDto>.Succeed(authResponseDto));

        // Act
        var result = await _sut.Register(request, CancellationToken.None);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(201);

        var response = objectResult.Value.Should().BeOfType<AuthResponse>().Subject;
        response.Token.Should().Be("jwt-token-here");
        response.TokenType.Should().Be("Bearer");
        response.Email.Should().Be("newuser@example.com");
        response.UserId.Should().Be(1);
        response.HasAthleteProfile.Should().BeFalse();
    }

    [Fact]
    public async Task Register_EmailAlreadyExists_Returns409Conflict()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        _authService.RegisterAsync(Arg.Any<RegisterDto>(), Arg.Any<CancellationToken>())
            .Returns(AuthResult<AuthResponseDto>.Fail(
                AuthErrorCodes.EmailExists,
                "An account with this email already exists."));

        // Act
        var result = await _sut.Register(request, CancellationToken.None);

        // Assert
        var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task Register_PasswordMismatch_Returns400BadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "user@example.com",
            Password = "Password123!",
            ConfirmPassword = "DifferentPassword456!"
        };

        _authService.RegisterAsync(Arg.Any<RegisterDto>(), Arg.Any<CancellationToken>())
            .Returns(AuthResult<AuthResponseDto>.Fail(
                AuthErrorCodes.PasswordMismatch,
                "Password confirmation does not match."));

        // Act
        var result = await _sut.Register(request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Register_MapsRequestToDto()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        RegisterDto? capturedDto = null;
        _authService.RegisterAsync(Arg.Do<RegisterDto>(dto => capturedDto = dto), Arg.Any<CancellationToken>())
            .Returns(AuthResult<AuthResponseDto>.Succeed(new AuthResponseDto
            {
                Token = "token",
                UserId = 1,
                Email = "newuser@example.com"
            }));

        // Act
        await _sut.Register(request, CancellationToken.None);

        // Assert
        capturedDto.Should().NotBeNull();
        capturedDto!.Email.Should().Be("newuser@example.com");
        capturedDto.Password.Should().Be("Password123!");
        capturedDto.ConfirmPassword.Should().Be("Password123!");
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithAuthResponse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "Password123!"
        };

        var authResponseDto = new AuthResponseDto
        {
            Token = "jwt-token-here",
            TokenType = "Bearer",
            ExpiresIn = 86400,
            UserId = 42,
            Email = "user@example.com",
            HasAthleteProfile = true,
            AthleteId = 10
        };

        _authService.LoginAsync(Arg.Any<LoginDto>(), Arg.Any<CancellationToken>())
            .Returns(AuthResult<AuthResponseDto>.Succeed(authResponseDto));

        // Act
        var result = await _sut.Login(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AuthResponse>().Subject;
        response.Token.Should().Be("jwt-token-here");
        response.Email.Should().Be("user@example.com");
        response.UserId.Should().Be(42);
        response.HasAthleteProfile.Should().BeTrue();
        response.AthleteId.Should().Be(10);
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401Unauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "WrongPassword"
        };

        _authService.LoginAsync(Arg.Any<LoginDto>(), Arg.Any<CancellationToken>())
            .Returns(AuthResult<AuthResponseDto>.Fail(
                AuthErrorCodes.InvalidCredentials,
                "Invalid email or password."));

        // Act
        var result = await _sut.Login(request, CancellationToken.None);

        // Assert
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Login_AccountDisabled_Returns401Unauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "disabled@example.com",
            Password = "Password123!"
        };

        _authService.LoginAsync(Arg.Any<LoginDto>(), Arg.Any<CancellationToken>())
            .Returns(AuthResult<AuthResponseDto>.Fail(
                AuthErrorCodes.AccountDisabled,
                "This account has been disabled."));

        // Act
        var result = await _sut.Login(request, CancellationToken.None);

        // Assert
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Login_MapsRequestToDto()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "Password123!"
        };

        LoginDto? capturedDto = null;
        _authService.LoginAsync(Arg.Do<LoginDto>(dto => capturedDto = dto), Arg.Any<CancellationToken>())
            .Returns(AuthResult<AuthResponseDto>.Succeed(new AuthResponseDto
            {
                Token = "token",
                UserId = 1,
                Email = "user@example.com"
            }));

        // Act
        await _sut.Login(request, CancellationToken.None);

        // Assert
        capturedDto.Should().NotBeNull();
        capturedDto!.Email.Should().Be("user@example.com");
        capturedDto.Password.Should().Be("Password123!");
    }

    [Fact]
    public async Task Login_UserWithoutAthleteProfile_ReturnsHasAthleteProfileFalse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "newuser@example.com",
            Password = "Password123!"
        };

        var authResponseDto = new AuthResponseDto
        {
            Token = "jwt-token",
            UserId = 1,
            Email = "newuser@example.com",
            HasAthleteProfile = false,
            AthleteId = null
        };

        _authService.LoginAsync(Arg.Any<LoginDto>(), Arg.Any<CancellationToken>())
            .Returns(AuthResult<AuthResponseDto>.Succeed(authResponseDto));

        // Act
        var result = await _sut.Login(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AuthResponse>().Subject;
        response.HasAthleteProfile.Should().BeFalse();
        response.AthleteId.Should().BeNull();
    }

    #endregion

    #region Response Format Tests

    [Fact]
    public async Task Register_SuccessResponse_IncludesTokenType()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "user@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        _authService.RegisterAsync(Arg.Any<RegisterDto>(), Arg.Any<CancellationToken>())
            .Returns(AuthResult<AuthResponseDto>.Succeed(new AuthResponseDto
            {
                Token = "token",
                TokenType = "Bearer",
                ExpiresIn = 86400,
                UserId = 1,
                Email = "user@example.com"
            }));

        // Act
        var result = await _sut.Register(request, CancellationToken.None);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        var response = objectResult.Value.Should().BeOfType<AuthResponse>().Subject;
        response.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public async Task Login_SuccessResponse_IncludesExpiresIn()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "Password123!"
        };

        _authService.LoginAsync(Arg.Any<LoginDto>(), Arg.Any<CancellationToken>())
            .Returns(AuthResult<AuthResponseDto>.Succeed(new AuthResponseDto
            {
                Token = "token",
                TokenType = "Bearer",
                ExpiresIn = 86400,
                UserId = 1,
                Email = "user@example.com"
            }));

        // Act
        var result = await _sut.Login(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AuthResponse>().Subject;
        response.ExpiresIn.Should().BeGreaterThan(0);
        response.ExpiresIn.Should().Be(86400); // 24 hours in seconds
    }

    #endregion
}
