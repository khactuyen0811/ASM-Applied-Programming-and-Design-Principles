using SIMS.Infrastructure.Csv;
using SIMS.Infrastructure.Factories;
using SIMS.Infrastructure.Options;
using SIMS.Infrastructure.Seeding;
using SIMS.Infrastructure.Security;

namespace SIMS.Tests.Unit.Infrastructure;

public sealed class CsvDataSeederTests : IDisposable
{
    private readonly string _directory;

    public CsvDataSeederTests()
    {
        _directory = Path.Combine(Path.GetTempPath(), "sims-seed-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, recursive: true);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private (CsvDataSeeder Seeder, CsvStudentRepository Students, CsvTeacherRepository Teachers, CsvCourseRepository Courses, CsvUserRepository Users, CsvEnrollmentRepository Enrollments) CreateSeeder()
    {
        var options = new CsvOptions { DataDirectory = _directory };
        var students = new CsvStudentRepository(options);
        var teachers = new CsvTeacherRepository(options);
        var courses = new CsvCourseRepository(options);
        var users = new CsvUserRepository(options);
        var enrollments = new CsvEnrollmentRepository(options);
        var hasher = new Pbkdf2PasswordHasher();
        var factory = new AppUserFactory(hasher);
        var seeder = new CsvDataSeeder(students, courses, enrollments, teachers, users, factory);

        return (seeder, students, teachers, courses, users, enrollments);
    }

    [Fact]
    public async Task SeedAsync_PopulatesExpectedCounts()
    {
        var context = CreateSeeder();

        await context.Seeder.SeedAsync();

        Assert.Equal(10_000, await context.Students.CountAsync());
        Assert.Equal(120, await context.Teachers.CountAsync());
        Assert.Equal(60, await context.Courses.CountAsync());
        Assert.True(await context.Users.CountAsync() >= 9);
        Assert.True(await context.Enrollments.CountAsync() >= 20_000);

        var admin = await context.Users.GetByUsernameAsync("trieunt");
        Assert.NotNull(admin);
        Assert.Equal(SIMS.Domain.Enums.UserRole.Admin, admin!.Role);
        Assert.True(admin.IsActive);
    }

    [Fact]
    public async Task SeedAsync_CalledTwice_DoesNotDuplicate()
    {
        var context = CreateSeeder();

        await context.Seeder.SeedAsync();
        var students = await context.Students.CountAsync();
        var teachers = await context.Teachers.CountAsync();
        var courses = await context.Courses.CountAsync();
        var users = await context.Users.CountAsync();
        var enrollments = await context.Enrollments.CountAsync();

        await context.Seeder.SeedAsync();

        Assert.Equal(students, await context.Students.CountAsync());
        Assert.Equal(teachers, await context.Teachers.CountAsync());
        Assert.Equal(courses, await context.Courses.CountAsync());
        Assert.Equal(users, await context.Users.CountAsync());
        Assert.Equal(enrollments, await context.Enrollments.CountAsync());
        Assert.Equal(10_000, students);
        Assert.Equal(120, teachers);
        Assert.Equal(60, courses);
    }
}
