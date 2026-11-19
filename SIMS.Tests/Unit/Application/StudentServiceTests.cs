using Moq;
using SIMS.Application.Dtos;
using SIMS.Application.Services;
using SIMS.Domain.Abstractions;
using SIMS.Domain.Entities;
using SIMS.Domain.Enums;

namespace SIMS.Tests.Unit.Application;

public sealed class StudentServiceTests
{
    private readonly Mock<IStudentRepository> _students = new();
    private readonly Mock<ICourseRepository> _courses = new();
    private readonly Mock<IEnrollmentRepository> _enrollments = new();

    private StudentService CreateSut() => new(_students.Object, _courses.Object, _enrollments.Object);

    private static Student Student(
        int id,
        string code,
        string name,
        string email,
        string program,
        StudentStatus status,
        int year = 2024) => new()
    {
        Id = id,
        StudentCode = code,
        FullName = name,
        Email = email,
        DateOfBirth = new DateTime(2003, 5, 1, 0, 0, 0, DateTimeKind.Utc),
        Gender = "Male",
        Phone = "0900000000",
        Address = "Ha Noi",
        Program = program,
        EnrollmentYear = year,
        Status = status,
        CreatedAt = new DateTime(2024, 9, 1, 0, 0, 0, DateTimeKind.Utc)
    };

    private static IReadOnlyList<Student> SampleStudents() => new List<Student>
    {
        Student(4, "SE202400004", "Dave Pham", "dave@example.com", "Data Science", StudentStatus.Graduated),
        Student(1, "SE202300001", "Alice Nguyen", "alice@example.com", "Software Engineering", StudentStatus.Active, 2023),
        Student(3, "SE202400003", "Carol Le", "carol@example.com", "Software Engineering", StudentStatus.Active, 2024),
        Student(2, "SE202300002", "Bob Tran", "bob@example.com", "Data Science", StudentStatus.Suspended, 2023)
    };

    private static StudentCreateRequest CreateRequest(string email = "new@example.com") => new(
        "New Student",
        new DateTime(2003, 1, 1),
        "Female",
        email,
        "0901234567",
        "Da Nang",
        "Software Engineering",
        2025);

    private static StudentUpdateRequest UpdateRequest(string email) => new(
        "Updated Name",
        new DateTime(2002, 2, 2),
        "Female",
        email,
        "0909999999",
        "Hue",
        "Data Science",
        2023,
        StudentStatus.Active);

    [Theory]
    [InlineData("alice", null, null, 1)]
    [InlineData("SE202300002", null, null, 1)]
    [InlineData(null, "Data Science", null, 2)]
    [InlineData(null, null, StudentStatus.Suspended, 1)]
    [InlineData(null, null, null, 4)]
    public async Task GetPagedAsync_FiltersBySearchProgramAndStatus(string? search, string? program, StudentStatus? status, int expected)
    {
        _students.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(SampleStudents());

        var result = await CreateSut().GetPagedAsync(search, program, status, 1, 20);

        Assert.Equal(expected, result.TotalCount);
        Assert.Equal(expected, result.Items.Count);
    }

    [Theory]
    [InlineData(0, 1000, 1, 200)]
    [InlineData(-5, -10, 1, 20)]
    [InlineData(2, 3, 2, 3)]
    public async Task GetPagedAsync_ClampsPageAndPageSize(int page, int pageSize, int expectedPage, int expectedPageSize)
    {
        _students.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(SampleStudents());

        var result = await CreateSut().GetPagedAsync(null, null, null, page, pageSize);

        Assert.Equal(expectedPage, result.Page);
        Assert.Equal(expectedPageSize, result.PageSize);
        Assert.Equal(expectedPageSize, result.PageSize <= 200 ? result.PageSize : 200);
    }

    [Fact]
    public async Task GetPagedAsync_OrdersByStudentCode()
    {
        _students.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(SampleStudents());

        var result = await CreateSut().GetPagedAsync(null, null, null, 1, 200);

        Assert.Equal(new[] { "SE202300001", "SE202300002", "SE202400003", "SE202400004" }, result.Items.Select(i => i.StudentCode));
    }

    [Fact]
    public async Task CreateAsync_RejectsInvalidEmail()
    {
        _students.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<Student>());
        _students.Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Student?)null);
        _students.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await CreateSut().CreateAsync(CreateRequest("not-an-email"));

        Assert.False(result.Succeeded);
        Assert.NotEmpty(result.Errors);
        _students.Verify(r => r.AddAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_RejectsDuplicateEmail()
    {
        _students.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<Student>());
        _students.Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Student?)null);
        _students.Setup(r => r.EmailExistsAsync("duplicate@example.com", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateSut().CreateAsync(CreateRequest("duplicate@example.com"));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Contains("Email is already in use", StringComparison.OrdinalIgnoreCase));
        _students.Verify(r => r.AddAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_GeneratesSequentialStudentCode()
    {
        _students.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<Student>());
        _students.Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Student?)null);
        _students.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await CreateSut().CreateAsync(CreateRequest());

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal("SE202500001", result.Value!.StudentCode);
        _students.Verify(r => r.AddAsync(It.Is<Student>(s => s.StudentCode == "SE202500001"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_PreservesStudentCode()
    {
        var existing = Student(5, "SE202400123", "Old Name", "old@example.com", "Software Engineering", StudentStatus.Active);
        _students.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _students.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Student> { existing });
        _students.Setup(r => r.EmailExistsAsync("new@example.com", 5, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _enrollments.Setup(r => r.GetByStudentIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<Enrollment>());

        var result = await CreateSut().UpdateAsync(5, UpdateRequest("new@example.com"));

        Assert.True(result.Succeeded);
        Assert.Equal("SE202400123", result.Value!.StudentCode);
        Assert.Equal("Updated Name", result.Value.FullName);
        _students.Verify(r => r.UpdateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_RejectsDuplicateEmail()
    {
        var existing = Student(5, "SE202400123", "Old Name", "old@example.com", "Software Engineering", StudentStatus.Active);
        _students.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _students.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Student> { existing });
        _students.Setup(r => r.EmailExistsAsync("taken@example.com", 5, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateSut().UpdateAsync(5, UpdateRequest("taken@example.com"));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Contains("Email is already in use", StringComparison.OrdinalIgnoreCase));
        _students.Verify(r => r.UpdateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_RemovesEnrollmentsThenStudent()
    {
        var existing = Student(5, "SE202400123", "Name", "x@example.com", "Software Engineering", StudentStatus.Active);
        _students.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _students.Setup(r => r.DeleteAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _enrollments.Setup(r => r.RemoveByStudentIdAsync(5, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await CreateSut().DeleteAsync(5);

        Assert.True(result);
        _enrollments.Verify(r => r.RemoveByStudentIdAsync(5, It.IsAny<CancellationToken>()), Times.Once);
        _students.Verify(r => r.DeleteAsync(5, It.IsAny<CancellationToken>()), Times.Once);
    }
}
