using SIMS.Domain.Entities;

namespace SIMS.Domain.Abstractions
{
    public interface IEnrollmentRepository : IRepository<Enrollment>
    {
        Task<IReadOnlyList<Enrollment>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Enrollment>> GetByCourseIdAsync(int courseId, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(int studentId, int courseId, CancellationToken cancellationToken = default);

        Task<int> CountByCourseIdAsync(int courseId, CancellationToken cancellationToken = default);

        Task RemoveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);
    }
}
