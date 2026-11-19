using SIMS.Domain.Entities;

namespace SIMS.Domain.Abstractions
{
    public interface IStudentRepository : IRepository<Student>
    {
        Task<Student?> GetByCodeAsync(string studentCode, CancellationToken cancellationToken = default);

        Task<bool> EmailExistsAsync(string email, int? excludeStudentId = null, CancellationToken cancellationToken = default);
    }
}
