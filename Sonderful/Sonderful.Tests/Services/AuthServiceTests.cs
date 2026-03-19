using Microsoft.Extensions.Configuration;
using Moq;
using Sonderful.API.DTOs.Auth;
using Sonderful.API.Models;
using Sonderful.API.Repositories;
using Sonderful.API.Services;

namespace Sonderful.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly IConfiguration _config;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "super-secret-test-key-that-is-long-enough-for-hmac"
            })
            .Build();

        _sut = new AuthService(_userRepoMock.Object, _config);
    }

    [Fact]
    public async Task Register_StoresHashedPassword_NotPlainText()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByEmailAsync("aoife@example.com"))
                     .ReturnsAsync((User?)null);

        User? captured = null;
        _userRepoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
                     .Callback<User>(u => captured = u)
                     .ReturnsAsync((User u) => u);

        var request = new RegisterRequest
        {
            Username = "aoife",
            Email = "aoife@example.com",
            Password = "plaintext123"
        };

        // Act
        await _sut.RegisterAsync(request);

        // Assert — password must be hashed, not the original string
        Assert.NotNull(captured);
        Assert.NotEqual("plaintext123", captured!.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("plaintext123", captured.PasswordHash));
    }

    [Fact]
    public async Task Register_ReturnsTokenAndUserId()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                     .ReturnsAsync((User?)null);

        _userRepoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
                     .ReturnsAsync((User u) => { u.Id = 42; return u; });

        var request = new RegisterRequest
        {
            Username = "oisin",
            Email = "oisin@example.com",
            Password = "password!"
        };

        // Act
        var response = await _sut.RegisterAsync(request);

        // Assert
        Assert.NotEmpty(response.Token);
        Assert.Equal(42, response.UserId);
        Assert.Equal("oisin", response.Username);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange — repository returns an existing user
        _userRepoMock.Setup(r => r.GetByEmailAsync("taken@example.com"))
                     .ReturnsAsync(new User { Id = 1, Email = "taken@example.com", Username = "existing" });

        var request = new RegisterRequest
        {
            Username = "newcomer",
            Email = "taken@example.com",
            Password = "password!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.RegisterAsync(request));
    }

    [Fact]
    public async Task Login_CorrectCredentials_ReturnsToken()
    {
        // Arrange
        var hash = BCrypt.Net.BCrypt.HashPassword("correctPass");
        _userRepoMock.Setup(r => r.GetByEmailAsync("siobhan@example.com"))
                     .ReturnsAsync(new User { Id = 7, Username = "siobhan", Email = "siobhan@example.com", PasswordHash = hash });

        var request = new LoginRequest { Email = "siobhan@example.com", Password = "correctPass" };

        // Act
        var response = await _sut.LoginAsync(request);

        // Assert
        Assert.NotEmpty(response.Token);
        Assert.Equal(7, response.UserId);
        Assert.Equal("siobhan", response.Username);
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var hash = BCrypt.Net.BCrypt.HashPassword("rightPassword");
        _userRepoMock.Setup(r => r.GetByEmailAsync("ciaran@example.com"))
                     .ReturnsAsync(new User { Id = 9, Username = "ciaran", Email = "ciaran@example.com", PasswordHash = hash });

        var request = new LoginRequest { Email = "ciaran@example.com", Password = "wrongPassword" };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.LoginAsync(request));
    }

    [Fact]
    public async Task Login_UnknownEmail_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                     .ReturnsAsync((User?)null);

        var request = new LoginRequest { Email = "nobody@example.com", Password = "anything" };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.LoginAsync(request));
    }
}
