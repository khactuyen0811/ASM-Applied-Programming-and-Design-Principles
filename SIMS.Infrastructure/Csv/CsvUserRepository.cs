using SIMS.Domain.Abstractions;
using SIMS.Domain.Entities;
using SIMS.Infrastructure.Options;

namespace SIMS.Infrastructure.Csv
{
    public sealed class CsvUserRepository : CsvRepository<AppUser>, IUserRepository
    {
        public CsvUserRepository(CsvOptions options)
            : base(options, CsvFileNames.Users)
        {
        }

        public async Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            var snapshot = await GetSnapshotAsync(cancellationToken).ConfigureAwait(false);
            foreach (var user in snapshot)
            {
                if (string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase))
                {
                    return user;
                }
            }

            return null;
        }
    }
}
