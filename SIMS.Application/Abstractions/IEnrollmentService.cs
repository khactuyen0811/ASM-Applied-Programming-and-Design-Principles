using SIMS.Application.Dtos;

namespace SIMS.Application.Abstractions
{
    public interface IEnrollmentService
    {
        Task<ServiceResult<bool>> EnrollAsync(int studentId, int courseId, CancellationToken cancellationToken = default);

        Task<bool> UnenrollAsync(int studentId, int courseId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<EnrollmentListItem>> GetByStudentAsync(int studentId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<EnrollmentListItem>> GetByCourseAsync(int courseId, CancellationToken cancellationToken = default);
    }
}
