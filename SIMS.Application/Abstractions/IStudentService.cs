using SIMS.Application.Dtos;
using SIMS.Domain.Common;
using SIMS.Domain.Enums;

namespace SIMS.Application.Abstractions
{
    public interface IStudentService
    {
        Task<PagedResult<StudentListItem>> GetPagedAsync(string? search, string? program, StudentStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);

        Task<PagedResult<StudentListItem>> GetPagedAsync(string? search, string? program, StudentStatus? status, int? year, int page, int pageSize, CancellationToken cancellationToken = default);

        Task<StudentDetails?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<ServiceResult<StudentDetails>> CreateAsync(StudentCreateRequest request, CancellationToken cancellationToken = default);

        Task<ServiceResult<StudentDetails>> UpdateAsync(int id, StudentUpdateRequest request, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<string>> GetProgramsAsync(CancellationToken cancellationToken = default);
    }
}
