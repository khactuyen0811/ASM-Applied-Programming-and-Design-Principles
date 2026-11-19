using SIMS.Domain.Abstractions;
using SIMS.Domain.Entities;
using SIMS.Infrastructure.Options;

namespace SIMS.Infrastructure.Csv
{
    public sealed class CsvTeacherRepository : CsvRepository<Teacher>, ITeacherRepository
    {
        public CsvTeacherRepository(CsvOptions options)
            : base(options, CsvFileNames.Teachers)
        {
        }

        public async Task<Teacher?> GetByCodeAsync(string teacherCode, CancellationToken cancellationToken = default)
        {
            var snapshot = await GetSnapshotAsync(cancellationToken).ConfigureAwait(false);
            foreach (var teacher in snapshot)
            {
                if (string.Equals(teacher.TeacherCode, teacherCode, StringComparison.OrdinalIgnoreCase))
                {
                    return teacher;
                }
            }

            return null;
        }
    }
}
