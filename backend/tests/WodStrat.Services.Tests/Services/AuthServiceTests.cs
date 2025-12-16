using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable.NSubstitute;
using NSubstitute;
using WodStrat.Dal.Interfaces;
using WodStrat.Dal.Models;
using WodStrat.Services.Configuration;
using WodStrat.Services.Dtos.Auth;
using WodStrat.Services.Services;
using WodStrat.Services.Tests.Customizations;
using Xunit;

namespace WodStrat.Services.Tests.Services;

/// <summary>
/// Unit tests for AuthService.
/// </summary>
public class AuthServiceTests
{
    private readonly IFixture _fixture;
    private readonly IWodStratDatabase _database;
    private readonly IOptions<JwtSettings> _jwtSettings;
    private readonly ILogger<AuthService> _logger;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new AthleteCustomization());

        _database = Substitute.For<IWodStratDatabase>();
        _logger = Substitute.For<ILogger<AuthService>>();

        var jwtSettings = new JwtSettings
        {
            SecretKey = "this-is-a-test-secret-key-minimum-32-characters-long",
            Issuer = "WodStrat-Test",
            Audience = "WodStrat-Test",
            ExpirationHours = 24
        };
        _jwtSettings = Options.Create(jwtSettings);

        _sut = new AuthService(_database, _jwtSettings, _logger);
    }

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_ValidCredentials_ReturnsSuccessWithToken()
    {
        // Arrange
        var dto = new RegisterDto
        {
            Email = "newuser@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var emptyUsers = Array.Empty<User>().AsQueryable().BuildMock();
        _database.Get<User>().Returns(emptyUsers);

        // Act
        var result = await _sut.RegisterAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().NotBeNullOrEmpty();
        result.Data.TokenType.Should().Be("Bearer");
        result.Data.Email.Should().Be("newuser@example.com");
        result.Data.HasAthleteProfile.Should().BeFalse();
        _database.Received(1).Add(Arg.Any<User>());
        await _database.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_EmailAlreadyExists_ReturnsFailure()
    {
        // Arrange
        var existingUser = new User
        {
            Id = 1,
            Email = "existing@example.com",
            PasswordHash = "hash",
            IsActive = true
        };

        var dto = new RegisterDto
        {
            Email = "existing@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var usersQueryable = new[] { existingUser }.AsQueryable().BuildMock();
        _database.Get<User>().Returns(usersQueryable);

        // Act
        var result = await _sut.RegisterAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(AuthErrorCodes.EmailExists);
        result.ErrorMessage.Should().Contain("email");
        _database.DidNotReceive().Add(Arg.Any<User>());
    }

    [Fact]
    public async Task RegisterAsync_PasswordMismatch_ReturnsFailure()
    {
        // Arrange
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "DifferentPassword456!"
        };

        var emptyUsers = Array.Empty<User>().AsQueryable().BuildMock();
        _database.Get<User>().Returns(emptyUsers);

        // Act
        var result = await _sut.RegisterAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(AuthErrorCodes.PasswordMismatch);
        _database.DidNotReceive().Add(Arg.Any<User>());
    }

    [Fact]
    public async Task RegisterAsync_NormalizesEmailToLowerCase()
    {
        // Arrange
        var dto = new RegisterDto
        {
            Email = "TestUser@EXAMPLE.COM",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var emptyUsers = Array.Empty<User>().AsQueryable().BuildMock();
        _database.Get<User>().Returns(emptyUsers);

        User? capturedUser = null;
        _database.When(x => x.Add(Arg.Any<User>()))
            .Do(callInfo => capturedUser = callInfo.Arg<User>());

        // Act
        var result = await _sut.RegisterAsync(dto);

        // Assert
        result.Success.Should().BeTrue();
        capturedUser.Should().NotBeNull();
        capturedUser!.Email.Should().Be("testuser@example.com");
    }

    [Fact]
    public async Task RegisterAsync_HashesPasswordWithBCrypt()
    {
        // Arrange
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var emptyUsers = Array.Empty<User>().AsQueryable().BuildMock();
        _database.Get<User>().Returns(emptyUsers);

        User? capturedUser = null;
        _database.When(x => x.Add(Arg.Any<User>()))
            .Do(callInfo => capturedUser = callInfo.Arg<User>());

        // Act
        await _sut.RegisterAsync(dto);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().NotBe(dto.Password);
        capturedUser.PasswordHash.Should().StartWith("$2"); // BCrypt hash prefix
        BCrypt.Net.BCrypt.Verify(dto.Password, capturedUser.PasswordHash).Should().BeTrue();
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccessWithToken()
    {
        // Arrange
        var password = "ValidPassword123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Id = 1,
            Email = "user@example.com",
            PasswordHash = passwordHash,
            IsActive = true,
            Athlete = null
        };

        var dto = new LoginDto
        {
            Email = "user@example.com",
            Password = password
        };

        var usersQueryable = new[] { user }.AsQueryable().BuildMock();
        _database.Get<User>().Returns(usersQueryable);

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().NotBeNullOrEmpty();
        result.Data.Email.Should().Be("user@example.com");
        result.Data.UserId.Should().Be(1);
        result.Data.HasAthleteProfile.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_ValidCredentialsWithAthlete_ReturnsAthleteInfo()
    {
        // Arrange
        var password = "ValidPassword123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var athlete = new Athlete
        {
            Id = 42,
            Name = "Test Athlete",
            IsDeleted = false
        };

        var user = new User
        {
            Id = 1,
            Email = "user@example.com",
            PasswordHash = passwordHash,
            IsActive = true,
            Athlete = athlete
        };

        var dto = new LoginDto
        {
            Email = "user@example.com",
            Password = password
        };

        var usersQueryable = new[] { user }.AsQueryable().BuildMock();
        _database.Get<User>().Returns(usersQueryable);

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.HasAthleteProfile.Should().BeTrue();
        result.Data.AthleteId.Should().Be(42);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsInvalidCredentials()
    {
        // Arrange
        var dto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        var emptyUsers = Array.Empty<User>().AsQueryable().BuildMock();
        _database.Get<User>().Returns(emptyUsers);

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(AuthErrorCodes.InvalidCredentials);
        result.ErrorMessage.Should().Contain("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsInvalidCredentials()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!"),
            IsActive = true
        };

        var dto = new LoginDto
        {
            Email = "user@example.com",
            Password = "WrongPassword456!"
        };

        var usersQueryable = new[] { user }.AsQueryable().BuildMock();
        _database.Get<User>().Returns(usersQueryable);

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(AuthErrorCodes.InvalidCredentials);
    }

    [Fact]
    public async Task LoginAsync_AccountDisabled_ReturnsAccountDisabled()
    {
        // Arrange
        var password = "Password123!";
        var user = new User
        {
            Id = 1,
            Email = "disabled@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsActive = false
        };

        var dto = new LoginDto
        {
            Email = "disabled@example.com",
            Password = password
        };

        var usersQueryable = new[] { user }.AsQueryable().BuildMock();
        _database.Get<User>().Returns(usersQueryable);

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(AuthErrorCodes.AccountDisabled);
    }

    [Fact]
    public async Task LoginAsync_NormalizesEmailToLowerCase()
    {
        // Arrange
        var password = "Password123!";
        var user = new User
        {
            Id = 1,
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsActive = true
        };

        var dto = new LoginDto
        {
            Email = "USER@EXAMPLE.COM",
            Password = password
        };

        var usersQueryable = new[] { user }.AsQueryable().BuildMock();
        _database.Get<User>().Returns(usersQueryable);

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task LoginAsync_DeletedAthlete_ReturnsHasAthleteProfileFalse()
    {
        // Arrange
        var password = "Password123!";
        var deletedAthlete = new Athlete
        {
            Id = 42,
            Name = "Deleted Athlete",
            IsDeleted = true
        };

        var user = new User
        {
            Id = 1,
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsActive = true,
            Athlete = deletedAthlete
        };

        var dto = new LoginDto
        {
            Email = "user@example.com",
            Password = password
        };

        var usersQueryable = new[] { user }.AsQueryable().BuildMock();
        _database.Get<User>().Returns(usersQueryable);

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.HasAthleteProfile.Should().BeFalse();
        result.Data.AthleteId.Should().BeNull();
    }

    #endregion

    #region IsEmailAvailableAsync Tests

    [Fact]
    public async Task IsEmailAvailableAsync_EmailNotTaken_ReturnsTrue()
    {
        // Arrange
        var emptyUsers = Array.Empty<User>().AsQueryable().BuildMock();
        _database.Get<User>().Returns(emptyUsers);

        // Act
        var result = await _sut.IsEmailAvailableAsync("new@example.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsEmailAvailableAsync_EmailTaken_ReturnsFalse()
    {
        // Arrange
        var existingUser = new User
        {
            Id = 1,
            Email = "existing@example.com"
        };

        var usersQueryable = new[] { existingUser }.AsQueryable().BuildMock();
        _database.Get<User>().Returns(usersQueryable);

        // Act
        var result = await _sut.IsEmailAvailableAsync("existing@example.com");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsEmailAvailableAsync_NormalizesEmailToLowerCase()
    {
        // Arrange
        var existingUser = new User
        {
            Id = 1,
            Email = "existing@example.com"
        };

        var usersQueryable = new[] { existingUser }.AsQueryable().BuildMock();
        _database.Get<User>().Returns(usersQueryable);

        // Act
        var result = await _sut.IsEmailAvailableAsync("EXISTING@EXAMPLE.COM");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region JWT Token Tests

    [Fact]
    public async Task RegisterAsync_GeneratesValidJwtToken()
    {
        // Arrange
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var emptyUsers = Array.Empty<User>().AsQueryable().BuildMock();
        _database.Get<User>().Returns(emptyUsers);

        // Act
        var result = await _sut.RegisterAsync(dto);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Token.Should().NotBeNullOrEmpty();

        // Verify token has three parts (header.payload.signature)
        var tokenParts = result.Data.Token.Split('.');
        tokenParts.Should().HaveCount(3);
    }

    [Fact]
    public async Task LoginAsync_TokenExpirationMatchesSettings()
    {
        // Arrange
        var password = "Password123!";
        var user = new User
        {
            Id = 1,
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsActive = true
        };

        var dto = new LoginDto
        {
            Email = "user@example.com",
            Password = password
        };

        var usersQueryable = new[] { user }.AsQueryable().BuildMock();
        _database.Get<User>().Returns(usersQueryable);

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.Success.Should().BeTrue();
        // 24 hours * 3600 seconds = 86400
        result.Data!.ExpiresIn.Should().Be(24 * 3600);
    }

    #endregion
}
