using SIMS.Domain.Abstractions;
using SIMS.Domain.Entities;
using SIMS.Infrastructure.Options;

namespace SIMS.Infrastructure.Csv
{
    public sealed class CsvEnrollmentRepository : CsvRepository<Enrollment>, IEnrollmentRepository
    {
        public CsvEnrollmentRepository(CsvOptions options)
            : base(options, CsvFileNames.Enrollments)
        {
        }

        public async Task<IReadOnlyList<Enrollment>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
        {
            var snapshot = await GetSnapshotAsync(cancellationToken).ConfigureAwait(false);
            var result = new List<Enrollment>();
            foreach (var enrollment in snapshot)
            {
                if (enrollment.StudentId == studentId)
                {
                    result.Add(enrollment);
                }
            }

            return result;
        }

        public async Task<IReadOnlyList<Enrollment>> GetByCourseIdAsync(int courseId, CancellationToken cancellationToken = default)
        {
            var snapshot = await GetSnapshotAsync(cancellationToken).ConfigureAwait(false);
            var result = new List<Enrollment>();
            foreach (var enrollment in snapshot)
            {
                if (enrollment.CourseId == courseId)
                {
                    result.Add(enrollment);
                }
            }

            return result;
        }

        public async Task<bool> ExistsAsync(int studentId, int courseId, CancellationToken cancellationToken = default)
        {
            var snapshot = await GetSnapshotAsync(cancellationToken).ConfigureAwait(false);
            foreach (var enrollment in snapshot)
            {
                if (enrollment.StudentId == studentId && enrollment.CourseId == courseId)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<int> CountByCourseIdAsync(int courseId, CancellationToken cancellationToken = default)
        {
            var snapshot = await GetSnapshotAsync(cancellationToken).ConfigureAwait(false);
            var count = 0;
            foreach (var enrollment in snapshot)
            {
                if (enrollment.CourseId == courseId)
                {
                    count++;
                }
            }

            return count;
        }

        public Task RemoveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
        {
            return MutateAsync(list => list.RemoveAll(enrollment => enrollment.StudentId == studentId) > 0, cancellationToken);
        }
    }
}
