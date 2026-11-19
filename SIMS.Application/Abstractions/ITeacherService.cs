using SIMS.Application.Dtos;
using SIMS.Domain.Common;

namespace SIMS.Application.Abstractions
{
    public interface ITeacherService
    {
        Task<IReadOnlyList<TeacherListItem>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<PagedResult<TeacherListItem>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);

        Task<TeacherDetails?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<ServiceResult<TeacherDetails>> CreateAsync(TeacherCreateRequest request, CancellationToken cancellationToken = default);

        Task<ServiceResult<TeacherDetails>> UpdateAsync(int id, TeacherUpdateRequest request, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
