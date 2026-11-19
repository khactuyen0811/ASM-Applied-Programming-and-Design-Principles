using SIMS.Application.Abstractions;
using SIMS.Application.Dtos;
using SIMS.Application.Internal;
using SIMS.Domain.Abstractions;
using SIMS.Domain.Common;
using SIMS.Domain.Entities;

namespace SIMS.Application.Services
{
    public sealed class TeacherService : ITeacherService
    {
        private readonly ITeacherRepository _teachers;
        private readonly ICourseRepository _courses;

        public TeacherService(ITeacherRepository teachers, ICourseRepository courses)
        {
            _teachers = teachers;
            _courses = courses;
        }

        public async Task<IReadOnlyList<TeacherListItem>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var all = await _teachers.GetAllAsync(cancellationToken);
            var items = new List<TeacherListItem>(all.Count);

            foreach (var teacher in all.OrderBy(t => t.TeacherCode, StringComparer.OrdinalIgnoreCase))
            {
                items.Add(await MapListItemAsync(teacher, cancellationToken));
            }

            return items;
        }

        public async Task<PagedResult<TeacherListItem>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var all = await _teachers.GetAllAsync(cancellationToken);
            IEnumerable<Teacher> query = all;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(t =>
                    t.TeacherCode.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || t.FullName.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || t.Email.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || t.Department.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            var filtered = query
                .OrderBy(t => t.TeacherCode, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var (normalizedPage, normalizedPageSize) = PagingHelper.Normalize(page, pageSize);
            var pageTeachers = filtered
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .ToList();

            var items = new List<TeacherListItem>(pageTeachers.Count);
            foreach (var teacher in pageTeachers)
            {
                items.Add(await MapListItemAsync(teacher, cancellationToken));
            }

            return new PagedResult<TeacherListItem>(items, normalizedPage, normalizedPageSize, filtered.Count);
        }

        public async Task<TeacherDetails?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var teacher = await _teachers.GetByIdAsync(id, cancellationToken);
            if (teacher is null)
            {
                return null;
            }

            return MapDetails(teacher);
        }

        public async Task<ServiceResult<TeacherDetails>> CreateAsync(TeacherCreateRequest request, CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                return ServiceResult<TeacherDetails>.Fail("Teacher request is required.");
            }

            var all = await _teachers.GetAllAsync(cancellationToken);
            var entity = new Teacher
            {
                TeacherCode = request.TeacherCode,
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                Department = request.Department,
                CreatedAt = DateTime.UtcNow
            };

            var errors = new List<string>(ValidationHelper.Validate(entity));
            ValidateUnique(entity, all, null, errors);

            if (errors.Count > 0)
            {
                return ServiceResult<TeacherDetails>.Fail(errors);
            }

            await _teachers.AddAsync(entity, cancellationToken);

            return ServiceResult<TeacherDetails>.Ok(MapDetails(entity));
        }

        public async Task<ServiceResult<TeacherDetails>> UpdateAsync(int id, TeacherUpdateRequest request, CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                return ServiceResult<TeacherDetails>.Fail("Teacher request is required.");
            }

            var teacher = await _teachers.GetByIdAsync(id, cancellationToken);
            if (teacher is null)
            {
                return ServiceResult<TeacherDetails>.Fail("Teacher not found.");
            }

            teacher.TeacherCode = request.TeacherCode;
            teacher.FullName = request.FullName;
            teacher.Email = request.Email;
            teacher.Phone = request.Phone;
            teacher.Department = request.Department;

            var all = await _teachers.GetAllAsync(cancellationToken);
            var errors = new List<string>(ValidationHelper.Validate(teacher));
            ValidateUnique(teacher, all, id, errors);

            if (errors.Count > 0)
            {
                return ServiceResult<TeacherDetails>.Fail(errors);
            }

            await _teachers.UpdateAsync(teacher, cancellationToken);

            var details = await GetByIdAsync(id, cancellationToken);
            if (details is null)
            {
                return ServiceResult<TeacherDetails>.Fail("Teacher not found.");
            }

            return ServiceResult<TeacherDetails>.Ok(details);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var teacher = await _teachers.GetByIdAsync(id, cancellationToken);
            if (teacher is null)
            {
                return false;
            }

            return await _teachers.DeleteAsync(id, cancellationToken);
        }

        private async Task<TeacherListItem> MapListItemAsync(Teacher teacher, CancellationToken cancellationToken)
        {
            var courses = await _courses.GetByLecturerIdAsync(teacher.Id, cancellationToken);

            return new TeacherListItem(
                teacher.Id,
                teacher.TeacherCode,
                teacher.FullName,
                teacher.Email,
                teacher.Department,
                courses.Count);
        }

        private static TeacherDetails MapDetails(Teacher teacher)
        {
            return new TeacherDetails(
                teacher.Id,
                teacher.TeacherCode,
                teacher.FullName,
                teacher.Email,
                teacher.Phone,
                teacher.Department,
                teacher.CreatedAt);
        }

        private static void ValidateUnique(Teacher teacher, IReadOnlyList<Teacher> all, int? excludeId, List<string> errors)
        {
            if (all.Any(t => t.Id != excludeId && string.Equals(t.TeacherCode, teacher.TeacherCode, StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add("Teacher code already exists.");
            }

            if (all.Any(t => t.Id != excludeId && string.Equals(t.Email, teacher.Email, StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add("Email is already in use by another teacher.");
            }
        }
    }
}
