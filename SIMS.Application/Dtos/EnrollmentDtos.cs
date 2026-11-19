namespace SIMS.Application.Dtos
{
    public sealed class EnrollmentListItem
    {
        public EnrollmentListItem()
        {
        }

        public EnrollmentListItem(
            int id,
            int studentId,
            string studentCode,
            string studentName,
            int courseId,
            string courseCode,
            string courseName,
            DateTime enrolledAt,
            double? grade)
        {
            Id = id;
            StudentId = studentId;
            StudentCode = studentCode;
            StudentName = studentName;
            CourseId = courseId;
            CourseCode = courseCode;
            CourseName = courseName;
            EnrolledAt = enrolledAt;
            Grade = grade;
        }

        public int Id { get; set; }

        public int StudentId { get; set; }

        public string StudentCode { get; set; } = string.Empty;

        public string StudentName { get; set; } = string.Empty;

        public int CourseId { get; set; }

        public string CourseCode { get; set; } = string.Empty;

        public string CourseName { get; set; } = string.Empty;

        public DateTime EnrolledAt { get; set; }

        public double? Grade { get; set; }
    }
}
