using SIMS.Domain.Entities;
using SIMS.Domain.Enums;
using SIMS.Infrastructure.Csv;
using SIMS.Infrastructure.Options;

namespace SIMS.Tests.Unit.Infrastructure;

public sealed class CsvStudentRepositoryTests : IDisposable
{
    private readonly string _directory;
    private readonly CsvOptions _options;

    public CsvStudentRepositoryTests()
    {
        _directory = Path.Combine(Path.GetTempPath(), "sims-csv-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
        _options = new CsvOptions { DataDirectory = _directory };
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

    private static Student NewStudent(string code, string email) => new()
    {
        StudentCode = code,
        FullName = "Nguyen Van A",
        DateOfBirth = new DateTime(2001, 2, 3, 4, 5, 6, DateTimeKind.Utc),
        Gender = "Male",
        Email = email,
        Phone = "0912345678",
        Address = "1 Ha Noi",
        Program = "Software Engineering",
        EnrollmentYear = 2024,
        Status = StudentStatus.Active,
        CreatedAt = new DateTime(2024, 9, 1, 0, 0, 0, DateTimeKind.Utc)
    };

    [Fact]
    public async Task AddAsync_AssignsIncreasingIds()
    {
        var repository = new CsvStudentRepository(_options);

        var first = NewStudent("SE202400001", "a@example.com");
        var second = NewStudent("SE202400002", "b@example.com");

        await repository.AddAsync(first);
        await repository.AddAsync(second);

        Assert.Equal(1, first.Id);
        Assert.Equal(2, second.Id);
        Assert.True(second.Id > first.Id);
        Assert.Equal(2, await repository.CountAsync());
    }

    [Fact]
    public async Task GetByIdAsync_RoundTripsAllFields()
    {
        var repository = new CsvStudentRepository(_options);
        var student = NewStudent("SE202400042", "roundtrip@example.com");
        student.Status = StudentStatus.Suspended;

        await repository.AddAsync(student);
        var loaded = await repository.GetByIdAsync(student.Id);

        Assert.NotNull(loaded);
        Assert.Equal(student.Id, loaded!.Id);
        Assert.Equal("SE202400042", loaded.StudentCode);
        Assert.Equal("Nguyen Van A", loaded.FullName);
        Assert.Equal(new DateTime(2001, 2, 3, 4, 5, 6, DateTimeKind.Utc), loaded.DateOfBirth);
        Assert.Equal(DateTimeKind.Utc, loaded.DateOfBirth.Kind);
        Assert.Equal("Male", loaded.Gender);
        Assert.Equal("roundtrip@example.com", loaded.Email);
        Assert.Equal("0912345678", loaded.Phone);
        Assert.Equal("1 Ha Noi", loaded.Address);
        Assert.Equal("Software Engineering", loaded.Program);
        Assert.Equal(2024, loaded.EnrollmentYear);
        Assert.Equal(StudentStatus.Suspended, loaded.Status);
        Assert.Equal(new DateTime(2024, 9, 1, 0, 0, 0, DateTimeKind.Utc), loaded.CreatedAt);
    }

    [Fact]
    public async Task UpdateAsync_PersistsAcrossNewRepositoryInstance()
    {
        var repository = new CsvStudentRepository(_options);
        var student = NewStudent("SE202400010", "before@example.com");
        await repository.AddAsync(student);

        student.FullName = "Tran Thi B";
        student.Email = "after@example.com";
        student.Program = "Data Science";
        student.Status = StudentStatus.Graduated;
        await repository.UpdateAsync(student);

        var reopened = new CsvStudentRepository(_options);
        var loaded = await reopened.GetByIdAsync(student.Id);

        Assert.NotNull(loaded);
        Assert.Equal("Tran Thi B", loaded!.FullName);
        Assert.Equal("after@example.com", loaded.Email);
        Assert.Equal("Data Science", loaded.Program);
        Assert.Equal(StudentStatus.Graduated, loaded.Status);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenIdMissing()
    {
        var repository = new CsvStudentRepository(_options);

        var deleted = await repository.DeleteAsync(999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task EmailExistsAsync_IsCaseInsensitive_AndHonoursExcludeId()
    {
        var repository = new CsvStudentRepository(_options);
        var student = NewStudent("SE202400011", "Alice@Example.com");
        await repository.AddAsync(student);
        await repository.AddAsync(NewStudent("SE202400012", "bob@example.com"));

        Assert.True(await repository.EmailExistsAsync("alice@example.com"));
        Assert.True(await repository.EmailExistsAsync("ALICE@EXAMPLE.COM"));
        Assert.False(await repository.EmailExistsAsync("alice@example.com", student.Id));
        Assert.True(await repository.EmailExistsAsync("alice@example.com", 999));
        Assert.False(await repository.EmailExistsAsync("nobody@example.com"));
    }

    [Fact]
    public async Task NewRepositoryInstance_SeesDataWrittenByPreviousInstance()
    {
        var writer = new CsvStudentRepository(_options);
        var student = NewStudent("SE202400020", "persist@example.com");
        await writer.AddAsync(student);

        var reader = new CsvStudentRepository(_options);
        var loaded = await reader.GetByIdAsync(student.Id);

        Assert.NotNull(loaded);
        Assert.Equal("persist@example.com", loaded!.Email);
        Assert.Null(await reader.GetByIdAsync(123456));
    }
}
