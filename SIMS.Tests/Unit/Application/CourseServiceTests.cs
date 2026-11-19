using Moq;
using SIMS.Application.Dtos;
using SIMS.Application.Services;
using SIMS.Domain.Abstractions;
using SIMS.Domain.Entities;

namespace SIMS.Tests.Unit.Application;

public sealed class CourseServiceTests
{
    private readonly Mock<ICourseRepository> _courses = new();
    private readonly Mock<ITeacherRepository> _teachers = new();
    private readonly Mock<IEnrollmentRepository> _enrollments = new();

    private CourseService CreateSut() => new(_courses.Object, _teachers.Object, _enrollments.Object);

    private static Course Course(int id, string code) => new()
    {
        Id = id,
        CourseCode = code,
        Name = "Existing Course",
        Description = "Existing",
        Credits = 3,
        Capacity = 30
    };

    private static CourseCreateRequest CreateRequest(string code, int? lecturerId = null) => new(
        code,
        "New Course",
        "Description",
        3,
        30,
        lecturerId);

    [Fact]
    public async Task CreateAsync_RejectsDuplicateCourseCode_CaseInsensitive()
    {
        _courses.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Course> { Course(1, "CRS001") });

        var result = await CreateSut().CreateAsync(CreateRequest("crs001"));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Contains("Course code already exists", StringComparison.OrdinalIgnoreCase));
        _courses.Verify(r => r.AddAsync(It.IsAny<Course>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_RejectsInvalidLecturerId()
    {
        _courses.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<Course>());
        _teachers.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Teacher?)null);

        var result = await CreateSut().CreateAsync(CreateRequest("CRS010", 99));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Contains("Lecturer does not exist", StringComparison.OrdinalIgnoreCase));
        _courses.Verify(r => r.AddAsync(It.IsAny<Course>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_DeletesRelatedEnrollments()
    {
        var course = Course(7, "CRS007");
        _courses.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(course);
        _courses.Setup(r => r.DeleteAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _enrollments.Setup(r => r.GetByCourseIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Enrollment>
            {
                new() { Id = 101, StudentId = 1, CourseId = 7 },
                new() { Id = 102, StudentId = 2, CourseId = 7 }
            });
        _enrollments.Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateSut().DeleteAsync(7);

        Assert.True(result);
        _enrollments.Verify(r => r.DeleteAsync(101, It.IsAny<CancellationToken>()), Times.Once);
        _enrollments.Verify(r => r.DeleteAsync(102, It.IsAny<CancellationToken>()), Times.Once);
        _courses.Verify(r => r.DeleteAsync(7, It.IsAny<CancellationToken>()), Times.Once);
    }
}
