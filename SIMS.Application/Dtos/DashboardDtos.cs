using SIMS.Domain.Enums;

namespace SIMS.Application.Dtos
{
    public sealed class ChartPoint
    {
        public ChartPoint()
        {
        }

        public ChartPoint(string label, int value)
        {
            Label = label;
            Value = value;
        }

        public string Label { get; set; } = string.Empty;

        public int Value { get; set; }
    }

    public sealed class DashboardStats
    {
        public DashboardStats()
        {
        }

        public DashboardStats(
            int totalStudents,
            int activeStudents,
            int totalCourses,
            int totalTeachers,
            int totalEnrollments,
            double averageEnrollmentsPerStudent,
            IReadOnlyList<ChartPoint> studentsByProgram,
            IReadOnlyList<ChartPoint> enrollmentsByCourse,
            IReadOnlyList<StudentListItem> recentStudents)
        {
            TotalStudents = totalStudents;
            ActiveStudents = activeStudents;
            TotalCourses = totalCourses;
            TotalTeachers = totalTeachers;
            TotalEnrollments = totalEnrollments;
            AverageEnrollmentsPerStudent = averageEnrollmentsPerStudent;
            StudentsByProgram = studentsByProgram ?? Array.Empty<ChartPoint>();
            EnrollmentsByCourse = enrollmentsByCourse ?? Array.Empty<ChartPoint>();
            RecentStudents = recentStudents ?? Array.Empty<StudentListItem>();
        }

        public int TotalStudents { get; set; }

        public int ActiveStudents { get; set; }

        public int TotalCourses { get; set; }

        public int TotalTeachers { get; set; }

        public int TotalEnrollments { get; set; }

        public double AverageEnrollmentsPerStudent { get; set; }

        public IReadOnlyList<ChartPoint> StudentsByProgram { get; set; } = Array.Empty<ChartPoint>();

        public IReadOnlyList<ChartPoint> EnrollmentsByCourse { get; set; } = Array.Empty<ChartPoint>();

        public IReadOnlyList<StudentListItem> RecentStudents { get; set; } = Array.Empty<StudentListItem>();
    }

    public sealed class FacultyCourseSummary
    {
        public FacultyCourseSummary()
        {
        }

        public FacultyCourseSummary(int courseId, string courseCode, string name, int credits, int capacity, int enrolledCount)
        {
            CourseId = courseId;
            CourseCode = courseCode;
            Name = name;
            Credits = credits;
            Capacity = capacity;
            EnrolledCount = enrolledCount;
        }

        public int CourseId { get; set; }

        public string CourseCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int Credits { get; set; }

        public int Capacity { get; set; }

        public int EnrolledCount { get; set; }
    }

    public sealed class FacultyDashboardStats
    {
        public FacultyDashboardStats()
        {
        }

        public FacultyDashboardStats(
            int teacherId,
            string teacherName,
            string department,
            IReadOnlyList<FacultyCourseSummary> courses,
            int totalStudents,
            int pendingGrades,
            IReadOnlyList<EnrollmentListItem> recentEnrollments)
        {
            TeacherId = teacherId;
            TeacherName = teacherName;
            Department = department;
            Courses = courses ?? Array.Empty<FacultyCourseSummary>();
            TotalStudents = totalStudents;
            PendingGrades = pendingGrades;
            RecentEnrollments = recentEnrollments ?? Array.Empty<EnrollmentListItem>();
        }

        public int TeacherId { get; set; }

        public string TeacherName { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public IReadOnlyList<FacultyCourseSummary> Courses { get; set; } = Array.Empty<FacultyCourseSummary>();

        public int TotalStudents { get; set; }

        public int PendingGrades { get; set; }

        public IReadOnlyList<EnrollmentListItem> RecentEnrollments { get; set; } = Array.Empty<EnrollmentListItem>();
    }

    public sealed class StudentCourseSummary
    {
        public StudentCourseSummary()
        {
        }

        public StudentCourseSummary(
            int courseId,
            string courseCode,
            string courseName,
            int credits,
            double? grade,
            DateTime enrolledAt,
            string lecturerName)
        {
            CourseId = courseId;
            CourseCode = courseCode;
            CourseName = courseName;
            Credits = credits;
            Grade = grade;
            EnrolledAt = enrolledAt;
            LecturerName = lecturerName;
        }

        public int CourseId { get; set; }

        public string CourseCode { get; set; } = string.Empty;

        public string CourseName { get; set; } = string.Empty;

        public int Credits { get; set; }

        public double? Grade { get; set; }

        public DateTime EnrolledAt { get; set; }

        public string LecturerName { get; set; } = string.Empty;
    }

    public sealed class StudentDashboardStats
    {
        public StudentDashboardStats()
        {
        }

        public StudentDashboardStats(
            int studentId,
            string studentCode,
            string fullName,
            string program,
            int enrollmentYear,
            StudentStatus status,
            double? gpa,
            int earnedCredits,
            int totalCredits,
            int courseCount,
            IReadOnlyList<StudentCourseSummary> courses)
        {
            StudentId = studentId;
            StudentCode = studentCode;
            FullName = fullName;
            Program = program;
            EnrollmentYear = enrollmentYear;
            Status = status;
            Gpa = gpa;
            EarnedCredits = earnedCredits;
            TotalCredits = totalCredits;
            CourseCount = courseCount;
            Courses = courses ?? Array.Empty<StudentCourseSummary>();
        }

        public int StudentId { get; set; }

        public string StudentCode { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Program { get; set; } = string.Empty;

        public int EnrollmentYear { get; set; }

        public StudentStatus Status { get; set; }

        public double? Gpa { get; set; }

        public int EarnedCredits { get; set; }

        public int TotalCredits { get; set; }

        public int CourseCount { get; set; }

        public IReadOnlyList<StudentCourseSummary> Courses { get; set; } = Array.Empty<StudentCourseSummary>();
    }
}
