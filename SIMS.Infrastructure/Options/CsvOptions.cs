namespace SIMS.Infrastructure.Options
{
    public sealed class CsvOptions
    {
        public string DataDirectory { get; init; } = string.Empty;
    }

    public static class CsvFileNames
    {
        public const string Students = "students.csv";

        public const string Courses = "courses.csv";

        public const string Enrollments = "enrollments.csv";

        public const string Users = "users.csv";

        public const string Teachers = "teachers.csv";
    }
}
