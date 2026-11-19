using SIMS.Application.Abstractions;
using SIMS.Application.Dtos;
using SIMS.Application.Internal;
using SIMS.Domain.Abstractions;
using SIMS.Domain.Common;
using SIMS.Domain.Entities;
using SIMS.Domain.Enums;

namespace SIMS.Application.Services
{
    public sealed class StudentService : IStudentService
    {
        private static readonly DateTime MinDateOfBirth = new DateTime(1900, 1, 1);

        private readonly IStudentRepository _students;
        private readonly ICourseRepository _courses;
        private readonly IEnrollmentRepository _enrollments;

        public StudentService(IStudentRepository students, ICourseRepository courses, IEnrollmentRepository enrollments)
        {
            _students = students;
            _courses = courses;
            _enrollments = enrollments;
        }

        public Task<PagedResult<StudentListItem>> GetPagedAsync(string? search, string? program, StudentStatus? status, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return GetPagedAsync(search, program, status, null, page, pageSize, cancellationToken);
        }

        public async Task<PagedResult<StudentListItem>> GetPagedAsync(string? search, string? program, StudentStatus? status, int? year, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var all = await _students.GetAllAsync(cancellationToken);
            IEnumerable<Student> query = all;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(s =>
                    s.StudentCode.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || s.FullName.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || s.Email.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(program))
            {
                var programFilter = program.Trim();
                query = query.Where(s => string.Equals(s.Program, programFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }

            if (year.HasValue)
            {
                query = query.Where(s => s.EnrollmentYear == year.Value);
            }

            var filtered = query
                .OrderBy(s => s.StudentCode, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var (normalizedPage, normalizedPageSize) = PagingHelper.Normalize(page, pageSize);

            var items = filtered
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .Select(MapListItem)
                .ToList();

            return new PagedResult<StudentListItem>(items, normalizedPage, normalizedPageSize, filtered.Count);
        }

        public async Task<StudentDetails?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var student = await _students.GetByIdAsync(id, cancellationToken);
            if (student is null)
            {
                return null;
            }

            var enrollments = await MapEnrollmentsAsync(student, cancellationToken);
            return MapDetails(student, enrollments);
        }

        public async Task<ServiceResult<StudentDetails>> CreateAsync(StudentCreateRequest request, CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                return ServiceResult<StudentDetails>.Fail("Student request is required.");
            }

            var existing = await _students.GetAllAsync(cancellationToken);
            var entity = new Student
            {
                FullName = request.FullName,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address,
                Program = request.Program,
                EnrollmentYear = request.EnrollmentYear,
                Status = StudentStatus.Active,
                CreatedAt = DateTime.UtcNow
            };
            entity.StudentCode = await GenerateStudentCodeAsync(entity.EnrollmentYear, existing, cancellationToken);

            var errors = new List<string>(ValidationHelper.Validate(entity));
            ValidateBusinessRules(entity, errors);

            if (await _students.EmailExistsAsync(entity.Email, null, cancellationToken))
            {
                errors.Add("Email is already in use by another student.");
            }

            if (errors.Count > 0)
            {
                return ServiceResult<StudentDetails>.Fail(errors);
            }

            await _students.AddAsync(entity, cancellationToken);

            return ServiceResult<StudentDetails>.Ok(MapDetails(entity, Array.Empty<EnrollmentListItem>()));
        }

        public async Task<ServiceResult<StudentDetails>> UpdateAsync(int id, StudentUpdateRequest request, CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                return ServiceResult<StudentDetails>.Fail("Student request is required.");
            }

            var student = await _students.GetByIdAsync(id, cancellationToken);
            if (student is null)
            {
                return ServiceResult<StudentDetails>.Fail("Student not found.");
            }

            student.FullName = request.FullName;
            student.DateOfBirth = request.DateOfBirth;
            student.Gender = request.Gender;
            student.Email = request.Email;
            student.Phone = request.Phone;
            student.Address = request.Address;
            student.Program = request.Program;
            student.EnrollmentYear = request.EnrollmentYear;
            student.Status = request.Status;

            var errors = new List<string>(ValidationHelper.Validate(student));
            ValidateBusinessRules(student, errors);

            if (await _students.EmailExistsAsync(student.Email, id, cancellationToken))
            {
                errors.Add("Email is already in use by another student.");
            }

            if (errors.Count > 0)
            {
                return ServiceResult<StudentDetails>.Fail(errors);
            }

            await _students.UpdateAsync(student, cancellationToken);

            var details = await GetByIdAsync(id, cancellationToken);
            if (details is null)
            {
                return ServiceResult<StudentDetails>.Fail("Student not found.");
            }

            return ServiceResult<StudentDetails>.Ok(details);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var student = await _students.GetByIdAsync(id, cancellationToken);
            if (student is null)
            {
                return false;
            }

            await _enrollments.RemoveByStudentIdAsync(id, cancellationToken);
            return await _students.DeleteAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<string>> GetProgramsAsync(CancellationToken cancellationToken = default)
        {
            var all = await _students.GetAllAsync(cancellationToken);

            return all
                .Select(s => s.Program)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private async Task<IReadOnlyList<EnrollmentListItem>> MapEnrollmentsAsync(Student student, CancellationToken cancellationToken)
        {
            var enrollments = await _enrollments.GetByStudentIdAsync(student.Id, cancellationToken);
            var items = new List<EnrollmentListItem>(enrollments.Count);

            foreach (var enrollment in enrollments.OrderByDescending(e => e.EnrolledAt))
            {
                var course = await _courses.GetByIdAsync(enrollment.CourseId, cancellationToken);
                items.Add(new EnrollmentListItem(
                    enrollment.Id,
                    enrollment.StudentId,
                    student.StudentCode,
                    student.FullName,
                    enrollment.CourseId,
                    course?.CourseCode ?? string.Empty,
                    course?.Name ?? string.Empty,
                    enrollment.EnrolledAt,
                    enrollment.Grade));
            }

            return items;
        }

        private static StudentListItem MapListItem(Student student)
        {
            return new StudentListItem(
                student.Id,
                student.StudentCode,
                student.FullName,
                student.Email,
                student.Program,
                student.EnrollmentYear,
                student.Status);
        }

        private static StudentDetails MapDetails(Student student, IReadOnlyList<EnrollmentListItem> enrollments)
        {
            return new StudentDetails(
                student.Id,
                student.StudentCode,
                student.FullName,
                student.DateOfBirth,
                student.Gender,
                student.Email,
                student.Phone,
                student.Address,
                student.Program,
                student.EnrollmentYear,
                student.Status,
                student.CreatedAt,
                enrollments);
        }

        private async Task<string> GenerateStudentCodeAsync(int enrollmentYear, IReadOnlyList<Student> existing, CancellationToken cancellationToken)
        {
            var maxSequence = 0;
            foreach (var student in existing)
            {
                var sequence = ExtractSequence(student.StudentCode);
                if (sequence > maxSequence)
                {
                    maxSequence = sequence;
                }
            }

            var next = maxSequence + 1;
            if (next < 1)
            {
                next = 1;
            }

            var existingCodes = new HashSet<string>(existing.Select(s => s.StudentCode), StringComparer.OrdinalIgnoreCase);

            while (true)
            {
                var candidate = $"SE{enrollmentYear}{next:D5}";
                if (!existingCodes.Contains(candidate) && await _students.GetByCodeAsync(candidate, cancellationToken) is null)
                {
                    return candidate;
                }

                next++;
            }
        }

        private static int ExtractSequence(string? code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return 0;
            }

            var index = code.Length;
            while (index > 0 && char.IsDigit(code[index - 1]))
            {
                index--;
            }

            var digits = code.Substring(index);
            if (digits.Length > 5)
            {
                digits = digits.Substring(digits.Length - 5);
            }

            return int.TryParse(digits, out var value) ? value : 0;
        }

        private static void ValidateBusinessRules(Student student, List<string> errors)
        {
            if (student.DateOfBirth.Date < MinDateOfBirth || student.DateOfBirth.Date > DateTime.Today)
            {
                errors.Add("Date of birth must be between 1900-01-01 and today.");
            }

            var currentYear = DateTime.UtcNow.Year;
            if (student.EnrollmentYear < 2000 || student.EnrollmentYear > currentYear + 1)
            {
                errors.Add($"Enrollment year must be between 2000 and {currentYear + 1}.");
            }
        }
    }
}
