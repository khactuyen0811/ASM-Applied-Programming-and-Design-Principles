using System.Security.Cryptography;
using SIMS.Application.Abstractions;
using SIMS.Application.Dtos;
using SIMS.Application.Internal;
using SIMS.Domain.Abstractions;
using SIMS.Domain.Common;
using SIMS.Domain.Entities;
using SIMS.Domain.Enums;

namespace SIMS.Application.Services
{
    public sealed class UserService : IUserService
    {
        private const int TemporaryPasswordLength = 12;

        private const string UppercaseCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ";

        private const string LowercaseCharacters = "abcdefghijkmnopqrstuvwxyz";

        private const string DigitCharacters = "23456789";

        private const string SymbolCharacters = "!@#$%^&*_-+=";

        private readonly IUserRepository _users;
        private readonly IStudentRepository _students;
        private readonly ITeacherRepository _teachers;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(
            IUserRepository users,
            IStudentRepository students,
            ITeacherRepository teachers,
            IPasswordHasher passwordHasher)
        {
            _users = users;
            _students = students;
            _teachers = teachers;
            _passwordHasher = passwordHasher;
        }

        public async Task<PagedResult<UserListItem>> GetPagedAsync(string? search, UserRole? role, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var all = await _users.GetAllAsync(cancellationToken);
            IEnumerable<AppUser> query = all;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(u =>
                    u.Username.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || u.DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            if (role.HasValue)
            {
                query = query.Where(u => u.Role == role.Value);
            }

            var filtered = query
                .OrderBy(u => u.Username, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var (normalizedPage, normalizedPageSize) = PagingHelper.Normalize(page, pageSize);
            var pageUsers = filtered
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .ToList();

            var items = new List<UserListItem>(pageUsers.Count);
            foreach (var user in pageUsers)
            {
                items.Add(await MapListItemAsync(user, cancellationToken));
            }

            return new PagedResult<UserListItem>(items, normalizedPage, normalizedPageSize, filtered.Count);
        }

        public async Task<UserDetails?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var user = await _users.GetByIdAsync(id, cancellationToken);
            if (user is null)
            {
                return null;
            }

            return await MapDetailsAsync(user, cancellationToken);
        }

        public async Task<UserSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            var all = await _users.GetAllAsync(cancellationToken);

            return new UserSummary(
                all.Count,
                all.Count(u => u.Role == UserRole.Admin),
                all.Count(u => u.Role == UserRole.Lecturer),
                all.Count(u => u.Role == UserRole.Student),
                all.Count(u => u.IsActive),
                all.Count(u => !u.IsActive));
        }

        public async Task<ServiceResult> UpdateRoleAsync(UpdateUserRoleRequest request, CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                return ServiceResult.Fail("User role request is required.");
            }

            if (!Enum.IsDefined(typeof(UserRole), request.Role))
            {
                return ServiceResult.Fail("Role is invalid.");
            }

            var user = await _users.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                return ServiceResult.Fail("User not found.");
            }

            if (user.IsActive
                && user.Role == UserRole.Admin
                && request.Role != UserRole.Admin)
            {
                var all = await _users.GetAllAsync(cancellationToken);
                var activeAdmins = all.Count(u => u.IsActive && u.Role == UserRole.Admin);
                if (activeAdmins <= 1)
                {
                    return ServiceResult.Fail("Cannot demote the last active administrator.");
                }
            }

            user.Role = request.Role;
            await _users.UpdateAsync(user, cancellationToken);

            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> SetActiveAsync(int userId, bool active, CancellationToken cancellationToken = default)
        {
            var user = await _users.GetByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                return ServiceResult.Fail("User not found.");
            }

            if (!active
                && user.IsActive
                && user.Role == UserRole.Admin)
            {
                var all = await _users.GetAllAsync(cancellationToken);
                var activeAdmins = all.Count(u => u.IsActive && u.Role == UserRole.Admin);
                if (activeAdmins <= 1)
                {
                    return ServiceResult.Fail("Cannot deactivate the last active administrator.");
                }
            }

            user.IsActive = active;
            await _users.UpdateAsync(user, cancellationToken);

            return ServiceResult.Ok();
        }

        public async Task<ServiceResult<ResetPasswordResult>> ResetPasswordAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _users.GetByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                return ServiceResult<ResetPasswordResult>.Fail("User not found.");
            }

            var temporaryPassword = GenerateTemporaryPassword();
            var hashed = _passwordHasher.Hash(temporaryPassword);
            user.PasswordHash = hashed.Hash;
            user.PasswordSalt = hashed.Salt;

            await _users.UpdateAsync(user, cancellationToken);

            return ServiceResult<ResetPasswordResult>.Ok(new ResetPasswordResult(user.Username, temporaryPassword));
        }

        public async Task<ServiceResult> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                return ServiceResult.Fail("Password change request is required.");
            }

            var user = await _users.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                return ServiceResult.Fail("User not found.");
            }

            if (!_passwordHasher.Verify(request.CurrentPassword ?? string.Empty, user.PasswordHash, user.PasswordSalt))
            {
                return ServiceResult.Fail("Current password is incorrect.");
            }

            var newPassword = request.NewPassword ?? string.Empty;
            if (newPassword.Length < 6)
            {
                return ServiceResult.Fail("New password must be at least 6 characters.");
            }

            if (string.Equals(newPassword, request.CurrentPassword, StringComparison.Ordinal))
            {
                return ServiceResult.Fail("New password must be different from the current password.");
            }

            var hashed = _passwordHasher.Hash(newPassword);
            user.PasswordHash = hashed.Hash;
            user.PasswordSalt = hashed.Salt;

            await _users.UpdateAsync(user, cancellationToken);

            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> UpdateDisplayNameAsync(int userId, string displayName, CancellationToken cancellationToken = default)
        {
            var user = await _users.GetByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                return ServiceResult.Fail("User not found.");
            }

            var trimmed = displayName?.Trim() ?? string.Empty;
            if (trimmed.Length == 0)
            {
                return ServiceResult.Fail("Display name is required.");
            }

            if (trimmed.Length > 120)
            {
                return ServiceResult.Fail("Display name must be 120 characters or fewer.");
            }

            user.DisplayName = trimmed;
            await _users.UpdateAsync(user, cancellationToken);

            return ServiceResult.Ok();
        }

        private async Task<UserListItem> MapListItemAsync(AppUser user, CancellationToken cancellationToken)
        {
            var (email, _) = await ResolveLinkAsync(user, cancellationToken);

            return new UserListItem(
                user.Id,
                user.Username,
                user.DisplayName,
                user.Role,
                user.IsActive,
                user.StudentId,
                user.TeacherId,
                email,
                user.CreatedAt);
        }

        private async Task<UserDetails> MapDetailsAsync(AppUser user, CancellationToken cancellationToken)
        {
            var (email, name) = await ResolveLinkAsync(user, cancellationToken);

            return new UserDetails(
                user.Id,
                user.Username,
                user.DisplayName,
                user.Role,
                user.IsActive,
                user.StudentId,
                user.TeacherId,
                email,
                name,
                user.CreatedAt);
        }

        private async Task<(string Email, string Name)> ResolveLinkAsync(AppUser user, CancellationToken cancellationToken)
        {
            if (user.StudentId.HasValue)
            {
                var student = await _students.GetByIdAsync(user.StudentId.Value, cancellationToken);
                if (student is not null)
                {
                    return (student.Email, student.FullName);
                }
            }

            if (user.TeacherId.HasValue)
            {
                var teacher = await _teachers.GetByIdAsync(user.TeacherId.Value, cancellationToken);
                if (teacher is not null)
                {
                    return (teacher.Email, teacher.FullName);
                }
            }

            return (string.Empty, string.Empty);
        }

        private static string GenerateTemporaryPassword()
        {
            var characters = new char[TemporaryPasswordLength];

            characters[0] = PickCharacter(UppercaseCharacters);
            characters[1] = PickCharacter(LowercaseCharacters);
            characters[2] = PickCharacter(DigitCharacters);
            characters[3] = PickCharacter(SymbolCharacters);

            var all = string.Concat(UppercaseCharacters, LowercaseCharacters, DigitCharacters, SymbolCharacters);
            for (var index = 4; index < characters.Length; index++)
            {
                characters[index] = PickCharacter(all);
            }

            for (var index = characters.Length - 1; index > 0; index--)
            {
                var swapIndex = RandomNumberGenerator.GetInt32(index + 1);
                (characters[index], characters[swapIndex]) = (characters[swapIndex], characters[index]);
            }

            return new string(characters);
        }

        private static char PickCharacter(string characters)
        {
            return characters[RandomNumberGenerator.GetInt32(characters.Length)];
        }
    }
}
