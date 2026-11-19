using System.Globalization;
using SIMS.Application.Abstractions;
using SIMS.Application.Dtos;
using SIMS.Domain.Abstractions;
using SIMS.Domain.Entities;
using SIMS.Domain.Enums;

namespace SIMS.Application.Services
{
    public sealed class DashboardService : IDashboardService
    {
        private readonly IStudentRepository _students;
        private readonly ICourseRepository _courses;
        private readonly ITeacherRepository _teachers;
        private readonly IEnrollmentRepository _enrollments;

        public DashboardService(
            IStudentRepository students,
            ICourseRepository courses,
            ITeacherRepository teachers,
            IEnrollmentRepository enrollments)
        {
            _students = students;
            _courses = courses;
            _teachers = teachers;
            _enrollments = enrollments;
        }

        public async Task<DashboardStats> GetStatsAsync(CancellationToken cancellationToken = default)
        {
            var students = await _students.GetAllAsync(cancellationToken);
            var courses = await _courses.GetAllAsync(cancellationToken);
            var teachers = await _teachers.GetAllAsync(cancellationToken);
            var enrollments = await _enrollments.GetAllAsync(cancellationToken);

            var totalStudents = students.Count;
            var totalEnrollments = enrollments.Count;
            var activeStudents = students.Count(s => s.Status == StudentStatus.Active);
            var averageEnrollments = Math.Round(totalEnrollments / (double)Math.Max(totalStudents, 1), 2);

            var studentsByProgram = students
                .Where(s => !string.IsNullOrWhiteSpace(s.Program))
                .GroupBy(s => s.Program.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => new ChartPoint(g.Key, g.Count()))
                .OrderByDescending(p => p.Value)
                .ThenBy(p => p.Label, StringComparer.OrdinalIgnoreCase)
                .Take(5)
                .ToList();

            var courseLookup = courses.ToDictionary(c => c.Id);

            var enrollmentsByCourse = enrollments
                .GroupBy(e => e.CourseId)
                .Select(g =>
                {
                    var label = courseLookup.TryGetValue(g.Key, out var course)
                        ? course.CourseCode
                        : g.Key.ToString();
                    return new ChartPoint(label, g.Count());
                })
                .OrderByDescending(p => p.Value)
                .ThenBy(p => p.Label, StringComparer.OrdinalIgnoreCase)
                .Take(5)
                .ToList();

            var recentStudents = students
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .Select(MapListItem)
                .ToList();

            return new DashboardStats(
                totalStudents,
                activeStudents,
                courses.Count,
                teachers.Count,
                totalEnrollments,
                averageEnrollments,
                studentsByProgram,
                enrollmentsByCourse,
                recentStudents);
        }

        public async Task<FacultyDashboardStats?> GetFacultyAsync(int teacherId, CancellationToken cancellationToken = default)
        {
            var teacher = await _teachers.GetByIdAsync(teacherId, cancellationToken);
            if (teacher is null)
            {
                return null;
            }

            var courses = await _courses.GetByLecturerIdAsync(teacher.Id, cancellationToken);
            var courseLookup = courses.ToDictionary(c => c.Id);
            var courseSummaries = new List<FacultyCourseSummary>(courses.Count);
            var facultyEnrollments = new List<Enrollment>();

            foreach (var course in courses.OrderBy(c => c.CourseCode, StringComparer.OrdinalIgnoreCase))
            {
                var enrolledCount = await _enrollments.CountByCourseIdAsync(course.Id, cancellationToken);
                courseSummaries.Add(new FacultyCourseSummary(
                    course.Id,
                    course.CourseCode,
                    course.Name,
                    course.Credits,
                    course.Capacity,
                    enrolledCount));

                var courseEnrollments = await _enrollments.GetByCourseIdAsync(course.Id, cancellationToken);
                facultyEnrollments.AddRange(courseEnrollments);
            }

            var totalStudents = facultyEnrollments
                .Select(e => e.StudentId)
                .Distinct()
                .Count();
            var pendingGrades = facultyEnrollments.Count(e => !e.Grade.HasValue);

            var recentEnrollments = new List<EnrollmentListItem>();
            foreach (var enrollment in facultyEnrollments
                .OrderByDescending(e => e.EnrolledAt)
                .Take(5))
            {
                var student = await _students.GetByIdAsync(enrollment.StudentId, cancellationToken);
                courseLookup.TryGetValue(enrollment.CourseId, out var course);

                recentEnrollments.Add(new EnrollmentListItem(
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

            return new FacultyDashboardStats(
                teacher.Id,
                teacher.FullName,
                teacher.Department,
                courseSummaries,
                totalStudents,
                pendingGrades,
                recentEnrollments);
        }

        public async Task<StudentDashboardStats?> GetStudentAsync(int studentId, CancellationToken cancellationToken = default)
        {
            var student = await _students.GetByIdAsync(studentId, cancellationToken);
            if (student is null)
            {
                return null;
            }

            var enrollments = await _enrollments.GetByStudentIdAsync(student.Id, cancellationToken);

            var grades = enrollments
                .Where(e => e.Grade.HasValue)
                .Select(e => e.Grade!.Value)
                .ToList();
            double? gpa = grades.Count > 0
                ? Math.Round(grades.Average(), 2)
                : null;

            var courseSummaries = new List<StudentCourseSummary>(enrollments.Count);
            var earnedCredits = 0;
            var totalCredits = 0;

            foreach (var enrollment in enrollments.OrderByDescending(e => e.EnrolledAt))
            {
                var course = await _courses.GetByIdAsync(enrollment.CourseId, cancellationToken);
                var lecturerName = string.Empty;

                if (course?.LecturerId is int lecturerId)
                {
                    var teacher = await _teachers.GetByIdAsync(lecturerId, cancellationToken);
                    lecturerName = teacher?.FullName ?? string.Empty;
                }

                var credits = course?.Credits ?? 0;
                totalCredits += credits;

                if (enrollment.Grade.HasValue && enrollment.Grade.Value >= 5)
                {
                    earnedCredits += credits;
                }

                courseSummaries.Add(new StudentCourseSummary(
                    enrollment.CourseId,
                    course?.CourseCode ?? string.Empty,
                    course?.Name ?? string.Empty,
                    credits,
                    enrollment.Grade,
                    enrollment.EnrolledAt,
                    lecturerName));
            }

            return new StudentDashboardStats(
                student.Id,
                student.StudentCode,
                student.FullName,
                student.Program,
                student.EnrollmentYear,
                student.Status,
                gpa,
                earnedCredits,
                totalCredits,
                enrollments.Count,
                courseSummaries);
        }

        public async Task<IReadOnlyList<ChartPoint>> GetEnrollmentTrendAsync(int months = 6, CancellationToken cancellationToken = default)
        {
            var normalizedMonths = Math.Clamp(months, 3, 12);
            var enrollments = await _enrollments.GetAllAsync(cancellationToken);

            var counts = enrollments
                .GroupBy(e => (e.EnrolledAt.Year, e.EnrolledAt.Month))
                .ToDictionary(g => g.Key, g => g.Count());

            var now = DateTime.UtcNow;
            var firstMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-(normalizedMonths - 1));
            var points = new List<ChartPoint>(normalizedMonths);

            for (var index = 0; index < normalizedMonths; index++)
            {
                var month = firstMonth.AddMonths(index);
                counts.TryGetValue((month.Year, month.Month), out var count);
                points.Add(new ChartPoint(month.ToString("MMM yyyy", CultureInfo.InvariantCulture), count));
            }

            return points;
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
    }
}
