using SIMS.Domain.Abstractions;
using SIMS.Domain.Entities;
using SIMS.Infrastructure.Options;

namespace SIMS.Infrastructure.Csv
{
    public sealed class CsvCourseRepository : CsvRepository<Course>, ICourseRepository
    {
        public CsvCourseRepository(CsvOptions options)
            : base(options, CsvFileNames.Courses)
        {
        }

        public async Task<Course?> GetByCodeAsync(string courseCode, CancellationToken cancellationToken = default)
        {
            var snapshot = await GetSnapshotAsync(cancellationToken).ConfigureAwait(false);
            foreach (var course in snapshot)
            {
                if (string.Equals(course.CourseCode, courseCode, StringComparison.OrdinalIgnoreCase))
                {
                    return course;
                }
            }

            return null;
        }

        public async Task<IReadOnlyList<Course>> GetByLecturerIdAsync(int teacherId, CancellationToken cancellationToken = default)
        {
            var snapshot = await GetSnapshotAsync(cancellationToken).ConfigureAwait(false);
            var result = new List<Course>();
            foreach (var course in snapshot)
            {
                if (course.LecturerId == teacherId)
                {
                    result.Add(course);
                }
            }

            return result;
        }
    }
}
