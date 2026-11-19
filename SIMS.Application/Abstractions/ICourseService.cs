using SIMS.Application.Dtos;
using SIMS.Domain.Common;

namespace SIMS.Application.Abstractions
{
    public interface ICourseService
    {
        Task<PagedResult<CourseListItem>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);

        Task<CourseDetails?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<ServiceResult<CourseDetails>> CreateAsync(CourseCreateRequest request, CancellationToken cancellationToken = default);

        Task<ServiceResult<CourseDetails>> UpdateAsync(int id, CourseUpdateRequest request, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<CourseListItem>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<CourseListItem>> GetByLecturerAsync(int teacherId, CancellationToken cancellationToken = default);
    }
}
