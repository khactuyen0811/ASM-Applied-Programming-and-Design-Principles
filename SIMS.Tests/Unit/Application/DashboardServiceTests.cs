using Moq;
using SIMS.Application.Services;
using SIMS.Domain.Abstractions;
using SIMS.Domain.Entities;
using SIMS.Domain.Enums;

namespace SIMS.Tests.Unit.Application;

public sealed class DashboardServiceTests
{
    private readonly Mock<IStudentRepository> _students = new();
    private readonly Mock<ICourseRepository> _courses = new();
    private readonly Mock<ITeacherRepository> _teachers = new();
    private readonly Mock<IEnrollmentRepository> _enrollments = new();

    private DashboardService CreateSut() => new(_students.Object, _courses.Object, _teachers.Object, _enrollments.Object);

    private static Student Student(int id, string program, StudentStatus status, DateTime createdAt) => new()
    {
        Id = id,
        StudentCode = $"SE20240000{id}",
        FullName = $"Student {id}",
        Email = $"s{id}@example.com",
        DateOfBirth = new DateTime(2003, 1, 1),
        Gender = "Male",
        Program = program,
        EnrollmentYear = 2024,
        Status = status,
        CreatedAt = createdAt
    };

    [Fact]
    public async Task GetStatsAsync_ReturnsCountsChartsAndRecentStudents()
    {
        var studentList = new List<Student>
        {
            Student(1, "Software Engineering", StudentStatus.Active, new DateTime(2024, 1, 1)),
            Student(2, "Software Engineering", StudentStatus.Active, new DateTime(2024, 2, 1)),
            Student(3, "Information Systems", StudentStatus.Suspended, new DateTime(2024, 3, 1)),
            Student(4, "Computer Science", StudentStatus.Graduated, new DateTime(2024, 4, 1))
        };
        var courseList = new List<Course>
        {
            new() { Id = 1, CourseCode = "CRS001", Name = "One", Credits = 3, Capacity = 30 },
            new() { Id = 2, CourseCode = "CRS002", Name = "Two", Credits = 3, Capacity = 30 },
            new() { Id = 3, CourseCode = "CRS003", Name = "Three", Credits = 3, Capacity = 30 }
        };
        var teacherList = new List<Teacher>
        {
            new() { Id = 1, TeacherCode = "GV0001", FullName = "T1", Email = "t1@example.com", Department = "Computing" },
            new() { Id = 2, TeacherCode = "GV0002", FullName = "T2", Email = "t2@example.com", Department = "Business" }
        };
        var enrollmentList = new List<Enrollment>
        {
            new() { Id = 1, StudentId = 1, CourseId = 1 },
            new() { Id = 2, StudentId = 2, CourseId = 1 },
            new() { Id = 3, StudentId = 3, CourseId = 1 },
            new() { Id = 4, StudentId = 4, CourseId = 1 },
            new() { Id = 5, StudentId = 1, CourseId = 2 },
            new() { Id = 6, StudentId = 2, CourseId = 2 },
            new() { Id = 7, StudentId = 3, CourseId = 99 }
        };

        _students.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(studentList);
        _courses.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(courseList);
        _teachers.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(teacherList);
        _enrollments.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(enrollmentList);

        var stats = await CreateSut().GetStatsAsync();

        Assert.Equal(4, stats.TotalStudents);
        Assert.Equal(2, stats.ActiveStudents);
        Assert.Equal(3, stats.TotalCourses);
        Assert.Equal(2, stats.TotalTeachers);
        Assert.Equal(7, stats.TotalEnrollments);
        Assert.Equal(1.75, stats.AverageEnrollmentsPerStudent);

        Assert.True(stats.StudentsByProgram.Count <= 5);
        Assert.Equal(3, stats.StudentsByProgram.Count);
        Assert.Equal("Software Engineering", stats.StudentsByProgram[0].Label);
        Assert.Equal(2, stats.StudentsByProgram[0].Value);

        Assert.True(stats.EnrollmentsByCourse.Count <= 5);
        Assert.Equal(3, stats.EnrollmentsByCourse.Count);
        Assert.Equal("CRS001", stats.EnrollmentsByCourse[0].Label);
        Assert.Equal(4, stats.EnrollmentsByCourse[0].Value);

        Assert.Equal(4, stats.RecentStudents.Count);
        Assert.Equal("SE202400004", stats.RecentStudents[0].StudentCode);
    }

    [Fact]
    public async Task GetStatsAsync_EmptyRepositories_DoesNotDivideByZero()
    {
        _students.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<Student>());
        _courses.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<Course>());
        _teachers.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<Teacher>());
        _enrollments.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<Enrollment>());

        var stats = await CreateSut().GetStatsAsync();

        Assert.Equal(0, stats.TotalStudents);
        Assert.Equal(0, stats.TotalCourses);
        Assert.Equal(0, stats.TotalTeachers);
        Assert.Equal(0, stats.TotalEnrollments);
        Assert.Equal(0, stats.AverageEnrollmentsPerStudent);
        Assert.Empty(stats.StudentsByProgram);
        Assert.Empty(stats.EnrollmentsByCourse);
        Assert.Empty(stats.RecentStudents);
    }
}
