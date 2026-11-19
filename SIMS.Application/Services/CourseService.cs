using SIMS.Application.Abstractions;
using SIMS.Application.Dtos;
using SIMS.Application.Internal;
using SIMS.Domain.Abstractions;
using SIMS.Domain.Common;
using SIMS.Domain.Entities;

namespace SIMS.Application.Services
{
    public sealed class CourseService : ICourseService
    {
        private readonly ICourseRepository _courses;
        private readonly ITeacherRepository _teachers;
        private readonly IEnrollmentRepository _enrollments;

        public CourseService(ICourseRepository courses, ITeacherRepository teachers, IEnrollmentRepository enrollments)
        {
            _courses = courses;
            _teachers = teachers;
            _enrollments = enrollments;
        }

        public async Task<PagedResult<CourseListItem>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var all = await _courses.GetAllAsync(cancellationToken);
            IEnumerable<Course> query = all;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(c =>
                    c.CourseCode.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || c.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            var filtered = query
                .OrderBy(c => c.CourseCode, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var (normalizedPage, normalizedPageSize) = PagingHelper.Normalize(page, pageSize);
            var pageCourses = filtered
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .ToList();

            var items = new List<CourseListItem>(pageCourses.Count);
            foreach (var course in pageCourses)
            {
                items.Add(await MapListItemAsync(course, cancellationToken));
            }

            return new PagedResult<CourseListItem>(items, normalizedPage, normalizedPageSize, filtered.Count);
        }

        public async Task<CourseDetails?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var course = await _courses.GetByIdAsync(id, cancellationToken);
            if (course is null)
            {
                return null;
            }

            return await MapDetailsAsync(course, cancellationToken);
        }

        public async Task<ServiceResult<CourseDetails>> CreateAsync(CourseCreateRequest request, CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                return ServiceResult<CourseDetails>.Fail("Course request is required.");
            }

            var all = await _courses.GetAllAsync(cancellationToken);
            var entity = new Course
            {
                CourseCode = request.CourseCode,
                Name = request.Name,
                Description = request.Description,
                Credits = request.Credits,
                Capacity = request.Capacity,
                LecturerId = request.LecturerId,
                CreatedAt = DateTime.UtcNow
            };

            var errors = new List<string>(ValidationHelper.Validate(entity));
            ValidateBusinessRules(entity, errors);

            if (all.Any(c => string.Equals(c.CourseCode, entity.CourseCode, StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add("Course code already exists.");
            }

            if (await LecturerMissingAsync(entity.LecturerId, cancellationToken))
            {
                errors.Add("Lecturer does not exist.");
            }

            if (errors.Count > 0)
            {
                return ServiceResult<CourseDetails>.Fail(errors);
            }

            await _courses.AddAsync(entity, cancellationToken);

            return ServiceResult<CourseDetails>.Ok(await MapDetailsAsync(entity, cancellationToken));
        }

        public async Task<ServiceResult<CourseDetails>> UpdateAsync(int id, CourseUpdateRequest request, CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                return ServiceResult<CourseDetails>.Fail("Course request is required.");
            }

            var course = await _courses.GetByIdAsync(id, cancellationToken);
            if (course is null)
            {
                return ServiceResult<CourseDetails>.Fail("Course not found.");
            }

            course.CourseCode = request.CourseCode;
            course.Name = request.Name;
            course.Description = request.Description;
            course.Credits = request.Credits;
            course.Capacity = request.Capacity;
            course.LecturerId = request.LecturerId;

            var all = await _courses.GetAllAsync(cancellationToken);
            var errors = new List<string>(ValidationHelper.Validate(course));
            ValidateBusinessRules(course, errors);

            if (all.Any(c => c.Id != id && string.Equals(c.CourseCode, course.CourseCode, StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add("Course code already exists.");
            }

            if (await LecturerMissingAsync(course.LecturerId, cancellationToken))
            {
                errors.Add("Lecturer does not exist.");
            }

            if (errors.Count > 0)
            {
                return ServiceResult<CourseDetails>.Fail(errors);
            }

            await _courses.UpdateAsync(course, cancellationToken);

            var details = await GetByIdAsync(id, cancellationToken);
            if (details is null)
            {
                return ServiceResult<CourseDetails>.Fail("Course not found.");
            }

            return ServiceResult<CourseDetails>.Ok(details);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var course = await _courses.GetByIdAsync(id, cancellationToken);
            if (course is null)
            {
                return false;
            }

            var enrollments = await _enrollments.GetByCourseIdAsync(id, cancellationToken);
            foreach (var enrollment in enrollments)
            {
                await _enrollments.DeleteAsync(enrollment.Id, cancellationToken);
            }

            return await _courses.DeleteAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<CourseListItem>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var all = await _courses.GetAllAsync(cancellationToken);
            var items = new List<CourseListItem>(all.Count);

            foreach (var course in all.OrderBy(c => c.CourseCode, StringComparer.OrdinalIgnoreCase))
            {
                items.Add(await MapListItemAsync(course, cancellationToken));
            }

            return items;
        }

        public async Task<IReadOnlyList<CourseListItem>> GetByLecturerAsync(int teacherId, CancellationToken cancellationToken = default)
        {
            var courses = await _courses.GetByLecturerIdAsync(teacherId, cancellationToken);
            var items = new List<CourseListItem>(courses.Count);

            foreach (var course in courses.OrderBy(c => c.CourseCode, StringComparer.OrdinalIgnoreCase))
            {
                items.Add(await MapListItemAsync(course, cancellationToken));
            }

            return items;
        }

        private async Task<bool> LecturerMissingAsync(int? lecturerId, CancellationToken cancellationToken)
        {
            if (!lecturerId.HasValue)
            {
                return false;
            }

            var teacher = await _teachers.GetByIdAsync(lecturerId.Value, cancellationToken);
            return teacher is null;
        }

        private async Task<string> GetLecturerNameAsync(int? lecturerId, CancellationToken cancellationToken)
        {
            if (!lecturerId.HasValue)
            {
                return string.Empty;
            }

            var teacher = await _teachers.GetByIdAsync(lecturerId.Value, cancellationToken);
            return teacher?.FullName ?? string.Empty;
        }

        private async Task<CourseListItem> MapListItemAsync(Course course, CancellationToken cancellationToken)
        {
            var enrolledCount = await _enrollments.CountByCourseIdAsync(course.Id, cancellationToken);
            var lecturerName = await GetLecturerNameAsync(course.LecturerId, cancellationToken);

            return new CourseListItem(
                course.Id,
                course.CourseCode,
                course.Name,
                course.Credits,
                course.Capacity,
                enrolledCount,
                lecturerName);
        }

        private async Task<CourseDetails> MapDetailsAsync(Course course, CancellationToken cancellationToken)
        {
            var enrolledCount = await _enrollments.CountByCourseIdAsync(course.Id, cancellationToken);
            var lecturerName = await GetLecturerNameAsync(course.LecturerId, cancellationToken);

            return new CourseDetails(
                course.Id,
                course.CourseCode,
                course.Name,
                course.Description,
                course.Credits,
                course.Capacity,
                course.LecturerId,
                lecturerName,
                enrolledCount);
        }

        private static void ValidateBusinessRules(Course course, List<string> errors)
        {
            if (course.Credits < 1 || course.Credits > 60)
            {
                errors.Add("Credits must be between 1 and 60.");
            }

            if (course.Capacity < 1 || course.Capacity > 1000)
            {
                errors.Add("Capacity must be between 1 and 1000.");
            }
        }
    }
}
