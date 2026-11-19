using SIMS.Application.Dtos;
using SIMS.Domain.Common;
using SIMS.Domain.Enums;

namespace SIMS.Application.Abstractions
{
    public interface IUserService
    {
        Task<PagedResult<UserListItem>> GetPagedAsync(string? search, UserRole? role, int page, int pageSize, CancellationToken cancellationToken = default);

        Task<UserDetails?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<UserSummary> GetSummaryAsync(CancellationToken cancellationToken = default);

        Task<ServiceResult> UpdateRoleAsync(UpdateUserRoleRequest request, CancellationToken cancellationToken = default);

        Task<ServiceResult> SetActiveAsync(int userId, bool active, CancellationToken cancellationToken = default);

        Task<ServiceResult<ResetPasswordResult>> ResetPasswordAsync(int userId, CancellationToken cancellationToken = default);

        Task<ServiceResult> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);

        Task<ServiceResult> UpdateDisplayNameAsync(int userId, string displayName, CancellationToken cancellationToken = default);
    }
}
