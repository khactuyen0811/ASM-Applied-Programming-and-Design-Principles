using SIMS.Domain.Entities;

namespace SIMS.Domain.Abstractions
{
    public interface IUserRepository : IRepository<AppUser>
    {
        Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    }
}
