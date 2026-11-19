using SIMS.Domain.Abstractions;
using SIMS.Domain.Entities;
using SIMS.Infrastructure.Options;

namespace SIMS.Infrastructure.Csv
{
    public sealed class CsvStudentRepository : CsvRepository<Student>, IStudentRepository
    {
        public CsvStudentRepository(CsvOptions options)
            : base(options, CsvFileNames.Students)
        {
        }

        public async Task<Student?> GetByCodeAsync(string studentCode, CancellationToken cancellationToken = default)
        {
            var snapshot = await GetSnapshotAsync(cancellationToken).ConfigureAwait(false);
            foreach (var student in snapshot)
            {
                if (string.Equals(student.StudentCode, studentCode, StringComparison.OrdinalIgnoreCase))
                {
                    return student;
                }
            }

            return null;
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeStudentId = null, CancellationToken cancellationToken = default)
        {
            var snapshot = await GetSnapshotAsync(cancellationToken).ConfigureAwait(false);
            foreach (var student in snapshot)
            {
                if (excludeStudentId.HasValue && student.Id == excludeStudentId.Value)
                {
                    continue;
                }

                if (string.Equals(student.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
