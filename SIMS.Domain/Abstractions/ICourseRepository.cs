using SIMS.Domain.Entities;

namespace SIMS.Domain.Abstractions
{
    public interface ICourseRepository : IRepository<Course>
    {
        Task<Course?> GetByCodeAsync(string courseCode, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Course>> GetByLecturerIdAsync(int teacherId, CancellationToken cancellationToken = default);
    }
}
