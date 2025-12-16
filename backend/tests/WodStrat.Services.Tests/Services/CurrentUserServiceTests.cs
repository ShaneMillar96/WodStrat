using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using WodStrat.Services.Services;
using Xunit;

namespace WodStrat.Services.Tests.Services;

/// <summary>
/// Unit tests for CurrentUserService.
/// </summary>
public class CurrentUserServiceTests
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CurrentUserService _sut;

    public CurrentUserServiceTests()
    {
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _sut = new CurrentUserService(_httpContextAccessor);
    }

    #region UserId Tests

    [Fact]
    public void UserId_AuthenticatedUser_ReturnsUserId()
    {
        // Arrange
        var userId = 42;
        var httpContext = CreateHttpContextWithUser(userId, "user@example.com");
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _sut.UserId;

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void UserId_NoHttpContext_ReturnsNull()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        var result = _sut.UserId;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void UserId_NoNameIdentifierClaim_ReturnsNull()
    {
        // Arrange
        var httpContext = CreateHttpContextWithClaims(new[]
        {
            new Claim(ClaimTypes.Email, "user@example.com")
        });
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _sut.UserId;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void UserId_InvalidUserIdClaim_ReturnsNull()
    {
        // Arrange
        var httpContext = CreateHttpContextWithClaims(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "not-a-number")
        });
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _sut.UserId;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void UserId_EmptyUserIdClaim_ReturnsNull()
    {
        // Arrange
        var httpContext = CreateHttpContextWithClaims(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "")
        });
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _sut.UserId;

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Email Tests

    [Fact]
    public void Email_AuthenticatedUser_ReturnsEmail()
    {
        // Arrange
        var email = "user@example.com";
        var httpContext = CreateHttpContextWithUser(42, email);
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _sut.Email;

        // Assert
        result.Should().Be(email);
    }

    [Fact]
    public void Email_NoHttpContext_ReturnsNull()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        var result = _sut.Email;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Email_NoEmailClaim_ReturnsNull()
    {
        // Arrange
        var httpContext = CreateHttpContextWithClaims(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "42")
        });
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _sut.Email;

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region IsAuthenticated Tests

    [Fact]
    public void IsAuthenticated_AuthenticatedUser_ReturnsTrue()
    {
        // Arrange
        var httpContext = CreateHttpContextWithUser(42, "user@example.com");
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _sut.IsAuthenticated;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAuthenticated_NoHttpContext_ReturnsFalse()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        var result = _sut.IsAuthenticated;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAuthenticated_UnauthenticatedUser_ReturnsFalse()
    {
        // Arrange
        var httpContext = CreateUnauthenticatedHttpContext();
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _sut.IsAuthenticated;

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetRequiredUserId Tests

    [Fact]
    public void GetRequiredUserId_AuthenticatedUser_ReturnsUserId()
    {
        // Arrange
        var userId = 42;
        var httpContext = CreateHttpContextWithUser(userId, "user@example.com");
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _sut.GetRequiredUserId();

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void GetRequiredUserId_NoHttpContext_ThrowsUnauthorizedException()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        var act = () => _sut.GetRequiredUserId();

        // Assert
        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("*not authenticated*");
    }

    [Fact]
    public void GetRequiredUserId_UnauthenticatedUser_ThrowsUnauthorizedException()
    {
        // Arrange
        var httpContext = CreateUnauthenticatedHttpContext();
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var act = () => _sut.GetRequiredUserId();

        // Assert
        act.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void GetRequiredUserId_NoUserIdClaim_ThrowsUnauthorizedException()
    {
        // Arrange
        var httpContext = CreateHttpContextWithClaims(new[]
        {
            new Claim(ClaimTypes.Email, "user@example.com")
        });
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var act = () => _sut.GetRequiredUserId();

        // Assert
        act.Should().Throw<UnauthorizedAccessException>();
    }

    #endregion

    #region Helper Methods

    private static HttpContext CreateHttpContextWithUser(int userId, string email)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email)
        };

        return CreateHttpContextWithClaims(claims, isAuthenticated: true);
    }

    private static HttpContext CreateHttpContextWithClaims(Claim[] claims, bool isAuthenticated = true)
    {
        var identity = new ClaimsIdentity(claims, isAuthenticated ? "Bearer" : null);
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        return httpContext;
    }

    private static HttpContext CreateUnauthenticatedHttpContext()
    {
        var identity = new ClaimsIdentity(); // No authentication type = unauthenticated
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        return httpContext;
    }

    #endregion
}
