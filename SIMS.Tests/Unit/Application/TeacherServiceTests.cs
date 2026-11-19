using Moq;
using SIMS.Application.Dtos;
using SIMS.Application.Services;
using SIMS.Domain.Abstractions;
using SIMS.Domain.Entities;

namespace SIMS.Tests.Unit.Application;

public sealed class TeacherServiceTests
{
    private readonly Mock<ITeacherRepository> _teachers = new();
    private readonly Mock<ICourseRepository> _courses = new();

    private TeacherService CreateSut() => new(_teachers.Object, _courses.Object);

    private static Teacher Teacher(int id, string code, string email) => new()
    {
        Id = id,
        TeacherCode = code,
        FullName = "Existing Teacher",
        Email = email,
        Phone = "0900000001",
        Department = "Computing",
        CreatedAt = new DateTime(2020, 1, 1)
    };

    [Fact]
    public async Task CreateAsync_RejectsDuplicateTeacherCode()
    {
        _teachers.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Teacher> { Teacher(2, "GV0001", "other@example.com") });

        var result = await CreateSut().CreateAsync(new TeacherCreateRequest("gv0001", "New Teacher", "new@example.com", "0901111111", "Computing"));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Contains("Teacher code already exists", StringComparison.OrdinalIgnoreCase));
        _teachers.Verify(r => r.AddAsync(It.IsAny<Teacher>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_RejectsDuplicateEmail()
    {
        _teachers.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Teacher> { Teacher(2, "GV0001", "dup@example.com") });

        var result = await CreateSut().CreateAsync(new TeacherCreateRequest("GV0002", "New Teacher", "DUP@EXAMPLE.COM", "0901111111", "Computing"));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Contains("Email is already in use", StringComparison.OrdinalIgnoreCase));
        _teachers.Verify(r => r.AddAsync(It.IsAny<Teacher>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_RejectsDuplicateCode_OnAnotherId()
    {
        var existing = Teacher(1, "GV0001", "one@example.com");
        var other = Teacher(2, "GV0002", "two@example.com");
        _teachers.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _teachers.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Teacher> { existing, other });

        var result = await CreateSut().UpdateAsync(1, new TeacherUpdateRequest("gv0002", "Updated", "one@example.com", "0900000001", "Business"));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Contains("Teacher code already exists", StringComparison.OrdinalIgnoreCase));
        _teachers.Verify(r => r.UpdateAsync(It.IsAny<Teacher>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
