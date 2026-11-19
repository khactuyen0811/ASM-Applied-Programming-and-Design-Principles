using Microsoft.EntityFrameworkCore;
using SIMS.Domain.Abstractions;
using SIMS.Domain.Entities;

namespace SIMS.Infrastructure.Data
{
    public sealed class EfStudentRepository : EfRepository<Student>, IStudentRepository
    {
        public EfStudentRepository(SimsDbContext context)
            : base(context)
        {
        }

        public async Task<Student?> GetByCodeAsync(string studentCode, CancellationToken cancellationToken = default)
        {
            return await Set.AsNoTracking()
                .FirstOrDefaultAsync(student => student.StudentCode == studentCode, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeStudentId = null, CancellationToken cancellationToken = default)
        {
            var query = Set.AsNoTracking().Where(student => student.Email == email);

            if (excludeStudentId.HasValue)
            {
                query = query.Where(student => student.Id != excludeStudentId.Value);
            }

            return await query.AnyAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
