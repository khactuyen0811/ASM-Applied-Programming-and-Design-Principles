using Microsoft.EntityFrameworkCore;
using SIMS.Domain.Abstractions;
using SIMS.Domain.Entities;

namespace SIMS.Infrastructure.Data
{
    public sealed class EfCourseRepository : EfRepository<Course>, ICourseRepository
    {
        public EfCourseRepository(SimsDbContext context)
            : base(context)
        {
        }

        public async Task<Course?> GetByCodeAsync(string courseCode, CancellationToken cancellationToken = default)
        {
            return await Set.AsNoTracking()
                .FirstOrDefaultAsync(course => course.CourseCode == courseCode, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<Course>> GetByLecturerIdAsync(int teacherId, CancellationToken cancellationToken = default)
        {
            return await Set.AsNoTracking()
                .Where(course => course.LecturerId == teacherId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
