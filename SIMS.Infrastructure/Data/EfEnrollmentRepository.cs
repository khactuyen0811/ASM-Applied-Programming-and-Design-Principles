using Microsoft.EntityFrameworkCore;
using SIMS.Domain.Abstractions;
using SIMS.Domain.Entities;

namespace SIMS.Infrastructure.Data
{
    public sealed class EfEnrollmentRepository : EfRepository<Enrollment>, IEnrollmentRepository
    {
        public EfEnrollmentRepository(SimsDbContext context)
            : base(context)
        {
        }

        public async Task<IReadOnlyList<Enrollment>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
        {
            return await Set.AsNoTracking()
                .Where(enrollment => enrollment.StudentId == studentId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<Enrollment>> GetByCourseIdAsync(int courseId, CancellationToken cancellationToken = default)
        {
            return await Set.AsNoTracking()
                .Where(enrollment => enrollment.CourseId == courseId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> ExistsAsync(int studentId, int courseId, CancellationToken cancellationToken = default)
        {
            return await Set.AsNoTracking()
                .AnyAsync(enrollment => enrollment.StudentId == studentId && enrollment.CourseId == courseId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<int> CountByCourseIdAsync(int courseId, CancellationToken cancellationToken = default)
        {
            return await Set.AsNoTracking()
                .CountAsync(enrollment => enrollment.CourseId == courseId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task RemoveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
        {
            var enrollments = await Set
                .Where(enrollment => enrollment.StudentId == studentId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (enrollments.Count == 0)
            {
                return;
            }

            Set.RemoveRange(enrollments);
            await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
