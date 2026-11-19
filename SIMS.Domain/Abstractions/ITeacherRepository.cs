using SIMS.Domain.Entities;

namespace SIMS.Domain.Abstractions
{
    public interface ITeacherRepository : IRepository<Teacher>
    {
        Task<Teacher?> GetByCodeAsync(string teacherCode, CancellationToken cancellationToken = default);
    }
}
