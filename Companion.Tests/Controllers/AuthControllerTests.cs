using Companion.Api.Controllers;
using Companion.Core.Models.Auth;
using Companion.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Companion.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var request = new LoginRequest("testuser", "password");
        var expectedResponse = new LoginResponse(
            "token123",
            "testuser",
            "User",
            DateTime.UtcNow.AddHours(1)
        );

        _authServiceMock.Setup(x => x.LoginAsync(request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<LoginResponse>(okResult.Value);
        Assert.Equal(expectedResponse.Token, response.Token);
        Assert.Equal(expectedResponse.Username, response.Username);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest("invalid", "password");
        _authServiceMock.Setup(x => x.LoginAsync(request))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

        // Act
        var result = await _controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsCreated()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        var expectedResponse = new RegisterResponse("newuser", "newuser@example.com", "User");
        _authServiceMock.Setup(x => x.RegisterAsync(request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<RegisterResponse>(createdResult.Value);
        Assert.Equal(expectedResponse.Username, response.Username);
        Assert.Equal(expectedResponse.Email, response.Email);
    }

    [Fact]
    public async Task Register_WithExistingUsername_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "existing",
            Email = "new@example.com",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        _authServiceMock.Setup(x => x.RegisterAsync(request))
            .ThrowsAsync(new InvalidOperationException("Username already exists"));

        // Act
        var result = await _controller.Register(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public void Validate_WithAuthenticatedUser_ReturnsOk()
    {
        // Act
        var result = _controller.Validate();

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
} 