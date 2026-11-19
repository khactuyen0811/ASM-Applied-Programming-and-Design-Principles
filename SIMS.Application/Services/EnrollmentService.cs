using SIMS.Application.Abstractions;
using SIMS.Application.Dtos;
using SIMS.Domain.Abstractions;
using SIMS.Domain.Entities;

namespace SIMS.Application.Services
{
    public sealed class EnrollmentService : IEnrollmentService
    {
        private readonly IEnrollmentRepository _enrollments;
        private readonly IStudentRepository _students;
        private readonly ICourseRepository _courses;

        public EnrollmentService(IEnrollmentRepository enrollments, IStudentRepository students, ICourseRepository courses)
        {
            _enrollments = enrollments;
            _students = students;
            _courses = courses;
        }

        public async Task<ServiceResult<bool>> EnrollAsync(int studentId, int courseId, CancellationToken cancellationToken = default)
        {
            var student = await _students.GetByIdAsync(studentId, cancellationToken);
            if (student is null)
            {
                return ServiceResult<bool>.Fail("Student not found.");
            }

            var course = await _courses.GetByIdAsync(courseId, cancellationToken);
            if (course is null)
            {
                return ServiceResult<bool>.Fail("Course not found.");
            }

            if (await _enrollments.ExistsAsync(studentId, courseId, cancellationToken))
            {
                return ServiceResult<bool>.Fail("Student is already enrolled in this course.");
            }

            var enrolledCount = await _enrollments.CountByCourseIdAsync(courseId, cancellationToken);
            if (enrolledCount >= course.Capacity)
            {
                return ServiceResult<bool>.Fail("Course is full.");
            }

            var enrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow
            };

            await _enrollments.AddAsync(enrollment, cancellationToken);

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<bool> UnenrollAsync(int studentId, int courseId, CancellationToken cancellationToken = default)
        {
            var enrollments = await _enrollments.GetByStudentIdAsync(studentId, cancellationToken);
            var enrollment = enrollments.FirstOrDefault(e => e.CourseId == courseId);
            if (enrollment is null)
            {
                return false;
            }

            return await _enrollments.DeleteAsync(enrollment.Id, cancellationToken);
        }

        public async Task<IReadOnlyList<EnrollmentListItem>> GetByStudentAsync(int studentId, CancellationToken cancellationToken = default)
        {
            var enrollments = await _enrollments.GetByStudentIdAsync(studentId, cancellationToken);
            return await MapAsync(enrollments, cancellationToken);
        }

        public async Task<IReadOnlyList<EnrollmentListItem>> GetByCourseAsync(int courseId, CancellationToken cancellationToken = default)
        {
            var enrollments = await _enrollments.GetByCourseIdAsync(courseId, cancellationToken);
            return await MapAsync(enrollments, cancellationToken);
        }

        private async Task<IReadOnlyList<EnrollmentListItem>> MapAsync(IReadOnlyList<Enrollment> enrollments, CancellationToken cancellationToken)
        {
            var studentCache = new Dictionary<int, Student?>();
            var courseCache = new Dictionary<int, Course?>();
            var items = new List<EnrollmentListItem>(enrollments.Count);

            foreach (var enrollment in enrollments.OrderByDescending(e => e.EnrolledAt))
            {
                if (!studentCache.TryGetValue(enrollment.StudentId, out var student))
                {
                    student = await _students.GetByIdAsync(enrollment.StudentId, cancellationToken);
                    studentCache[enrollment.StudentId] = student;
                }

                if (!courseCache.TryGetValue(enrollment.CourseId, out var course))
                {
                    course = await _courses.GetByIdAsync(enrollment.CourseId, cancellationToken);
                    courseCache[enrollment.CourseId] = course;
                }

                items.Add(new EnrollmentListItem(
                    enrollment.Id,
                    enrollment.StudentId,
                    student?.StudentCode ?? string.Empty,
                    student?.FullName ?? string.Empty,
                    enrollment.CourseId,
                    course?.CourseCode ?? string.Empty,
                    course?.Name ?? string.Empty,
                    enrollment.EnrolledAt,
                    enrollment.Grade));
            }

            return items;
        }
    }
}
