using SIMS.Application.Abstractions;
using SIMS.Application.Dtos;
using SIMS.Domain.Abstractions;

namespace SIMS.Application.Services
{
    public sealed class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(IUserRepository users, IPasswordHasher passwordHasher)
        {
            _users = users;
            _passwordHasher = passwordHasher;
        }

        public async Task<AuthenticatedUser?> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var user = await _users.GetByUsernameAsync(username.Trim(), cancellationToken);
            if (user is null || !user.IsActive)
            {
                return null;
            }

            if (!_passwordHasher.Verify(password, user.PasswordHash, user.PasswordSalt))
            {
                return null;
            }

            return new AuthenticatedUser(user.Id, user.Username, user.DisplayName, user.Role, user.StudentId, user.TeacherId);
        }
    }
}
