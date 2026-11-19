using SIMS.Application.Dtos;

namespace SIMS.Application.Abstractions
{
    public interface IAuthService
    {
        Task<AuthenticatedUser?> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
    }
}
