using SIMS.Domain.Entities;
using SIMS.Domain.Enums;

namespace SIMS.Domain.Abstractions
{
    public interface IUserFactory
    {
        AppUser Create(
            string username,
            string password,
            string displayName,
            UserRole role,
            int? studentId = null,
            int? teacherId = null,
            string? passwordHash = null,
            string? passwordSalt = null);
    }
}
