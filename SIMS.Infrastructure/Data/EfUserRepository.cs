using Microsoft.EntityFrameworkCore;
using SIMS.Domain.Abstractions;
using SIMS.Domain.Entities;

namespace SIMS.Infrastructure.Data
{
    public sealed class EfUserRepository : EfRepository<AppUser>, IUserRepository
    {
        public EfUserRepository(SimsDbContext context)
            : base(context)
        {
        }

        public async Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await Set.AsNoTracking()
                .FirstOrDefaultAsync(user => user.Username == username, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
