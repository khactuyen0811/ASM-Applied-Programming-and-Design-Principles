using Moq;
using SIMS.Application.Services;
using SIMS.Domain.Abstractions;
using SIMS.Domain.Entities;
using SIMS.Domain.Enums;

namespace SIMS.Tests.Unit.Application;

public sealed class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();

    private AuthService CreateSut() => new(_users.Object, _hasher.Object);

    private static AppUser User(bool isActive = true) => new()
    {
        Id = 7,
        Username = "trieunt",
        PasswordHash = "stored-hash",
        PasswordSalt = "stored-salt",
        DisplayName = "Thanh Trieu",
        Role = UserRole.Admin,
        StudentId = null,
        TeacherId = null,
        IsActive = isActive
    };

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthenticatedUser()
    {
        var user = User();
        _users.Setup(r => r.GetByUsernameAsync("trieunt", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("12345", "stored-hash", "stored-salt")).Returns(true);

        var result = await CreateSut().LoginAsync("trieunt", "12345");

        Assert.NotNull(result);
        Assert.Equal(7, result!.Id);
        Assert.Equal("trieunt", result.Username);
        Assert.Equal("Thanh Trieu", result.DisplayName);
        Assert.Equal(UserRole.Admin, result.Role);
        Assert.Null(result.StudentId);
        Assert.Null(result.TeacherId);
    }

    [Fact]
    public async Task LoginAsync_ValidStudentCredentials_ReturnsStudentId()
    {
        var user = User();
        user.Role = UserRole.Student;
        user.StudentId = 42;
        _users.Setup(r => r.GetByUsernameAsync("student", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("12345", "stored-hash", "stored-salt")).Returns(true);

        var result = await CreateSut().LoginAsync("student", "12345");

        Assert.NotNull(result);
        Assert.Equal(UserRole.Student, result!.Role);
        Assert.Equal(42, result.StudentId);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsNull()
    {
        var user = User();
        _users.Setup(r => r.GetByUsernameAsync("trieunt", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("wrong", "stored-hash", "stored-salt")).Returns(false);

        var result = await CreateSut().LoginAsync("trieunt", "wrong");

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_UnknownUser_ReturnsNull()
    {
        _users.Setup(r => r.GetByUsernameAsync("ghost", It.IsAny<CancellationToken>())).ReturnsAsync((AppUser?)null);

        var result = await CreateSut().LoginAsync("ghost", "12345");

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ReturnsNull()
    {
        var user = User(isActive: false);
        _users.Setup(r => r.GetByUsernameAsync("trieunt", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await CreateSut().LoginAsync("trieunt", "12345");

        Assert.Null(result);
        _hasher.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
