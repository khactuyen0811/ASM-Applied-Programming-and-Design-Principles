using SIMS.Application.Dtos;

namespace SIMS.Application.Abstractions
{
    public interface IDashboardService
    {
        Task<DashboardStats> GetStatsAsync(CancellationToken cancellationToken = default);

        Task<FacultyDashboardStats?> GetFacultyAsync(int teacherId, CancellationToken cancellationToken = default);

        Task<StudentDashboardStats?> GetStudentAsync(int studentId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ChartPoint>> GetEnrollmentTrendAsync(int months = 6, CancellationToken cancellationToken = default);
    }
}
