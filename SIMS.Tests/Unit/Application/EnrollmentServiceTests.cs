using Moq;
using SIMS.Application.Services;
using SIMS.Domain.Abstractions;
using SIMS.Domain.Entities;

namespace SIMS.Tests.Unit.Application;

public sealed class EnrollmentServiceTests
{
    private readonly Mock<IEnrollmentRepository> _enrollments = new();
    private readonly Mock<IStudentRepository> _students = new();
    private readonly Mock<ICourseRepository> _courses = new();

    private EnrollmentService CreateSut() => new(_enrollments.Object, _students.Object, _courses.Object);

    private static Student Student(int id = 1) => new()
    {
        Id = id,
        StudentCode = "SE202400001",
        FullName = "Student One",
        Email = "s1@example.com",
        DateOfBirth = new DateTime(2003, 1, 1),
        Gender = "Male",
        Program = "Software Engineering",
        EnrollmentYear = 2024
    };

    private static Course Course(int id = 1, int capacity = 30) => new()
    {
        Id = id,
        CourseCode = "CRS001",
        Name = "Programming",
        Credits = 3,
        Capacity = capacity
    };

    [Fact]
    public async Task EnrollAsync_Fails_WhenStudentMissing()
    {
        _students.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Student?)null);

        var result = await CreateSut().EnrollAsync(1, 1);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Contains("Student not found", StringComparison.OrdinalIgnoreCase));
        _enrollments.Verify(r => r.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EnrollAsync_Fails_WhenCourseMissing()
    {
        _students.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Student());
        _courses.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Course?)null);

        var result = await CreateSut().EnrollAsync(1, 1);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Contains("Course not found", StringComparison.OrdinalIgnoreCase));
        _enrollments.Verify(r => r.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EnrollAsync_Fails_WhenAlreadyEnrolled()
    {
        _students.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Student());
        _courses.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Course());
        _enrollments.Setup(r => r.ExistsAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateSut().EnrollAsync(1, 1);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Contains("already enrolled", StringComparison.OrdinalIgnoreCase));
        _enrollments.Verify(r => r.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EnrollAsync_Fails_WhenCourseFull()
    {
        _students.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Student());
        _courses.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Course(capacity: 2));
        _enrollments.Setup(r => r.ExistsAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _enrollments.Setup(r => r.CountByCourseIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(2);

        var result = await CreateSut().EnrollAsync(1, 1);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Contains("full", StringComparison.OrdinalIgnoreCase));
        _enrollments.Verify(r => r.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EnrollAsync_Succeeds_WhenValid()
    {
        _students.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Student());
        _courses.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Course(capacity: 2));
        _enrollments.Setup(r => r.ExistsAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _enrollments.Setup(r => r.CountByCourseIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await CreateSut().EnrollAsync(1, 1);

        Assert.True(result.Succeeded);
        Assert.True(result.Value);
        _enrollments.Verify(r => r.AddAsync(It.Is<Enrollment>(e => e.StudentId == 1 && e.CourseId == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnenrollAsync_ReturnsFalse_WhenNoEnrollmentExists()
    {
        _enrollments.Setup(r => r.GetByStudentIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Enrollment> { new() { Id = 9, StudentId = 1, CourseId = 2 } });

        var result = await CreateSut().UnenrollAsync(1, 5);

        Assert.False(result);
        _enrollments.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UnenrollAsync_DeletesMatchingEnrollment()
    {
        _enrollments.Setup(r => r.GetByStudentIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Enrollment> { new() { Id = 9, StudentId = 1, CourseId = 5 } });
        _enrollments.Setup(r => r.DeleteAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateSut().UnenrollAsync(1, 5);

        Assert.True(result);
        _enrollments.Verify(r => r.DeleteAsync(9, It.IsAny<CancellationToken>()), Times.Once);
    }
}
