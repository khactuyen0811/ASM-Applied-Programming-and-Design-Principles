using Microsoft.EntityFrameworkCore;
using SIMS.Domain.Abstractions;
using SIMS.Domain.Entities;

namespace SIMS.Infrastructure.Data
{
    public sealed class EfTeacherRepository : EfRepository<Teacher>, ITeacherRepository
    {
        public EfTeacherRepository(SimsDbContext context)
            : base(context)
        {
        }

        public async Task<Teacher?> GetByCodeAsync(string teacherCode, CancellationToken cancellationToken = default)
        {
            return await Set.AsNoTracking()
                .FirstOrDefaultAsync(teacher => teacher.TeacherCode == teacherCode, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
