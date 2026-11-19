using SIMS.Domain.Abstractions;
using SIMS.Domain.Entities;
using SIMS.Domain.Enums;

namespace SIMS.Infrastructure.Factories
{
    public sealed class AppUserFactory : IUserFactory
    {
        private readonly IPasswordHasher _passwordHasher;

        public AppUserFactory(IPasswordHasher passwordHasher)
        {
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public AppUser Create(
            string username,
            string password,
            string displayName,
            UserRole role,
            int? studentId = null,
            int? teacherId = null,
            string? passwordHash = null,
            string? passwordSalt = null)
        {
            string hash;
            string salt;

            if (!string.IsNullOrEmpty(passwordHash) && !string.IsNullOrEmpty(passwordSalt))
            {
                hash = passwordHash;
                salt = passwordSalt;
            }
            else
            {
                var result = _passwordHasher.Hash(password);
                hash = result.Hash;
                salt = result.Salt;
            }

            return new AppUser
            {
                Username = username,
                PasswordHash = hash,
                PasswordSalt = salt,
                DisplayName = displayName,
                Role = role,
                StudentId = studentId,
                TeacherId = teacherId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
