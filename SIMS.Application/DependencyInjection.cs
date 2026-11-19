using Microsoft.Extensions.DependencyInjection;
using SIMS.Application.Abstractions;
using SIMS.Application.Services;

namespace SIMS.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSimsApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<ITeacherService, TeacherService>();
            services.AddScoped<IEnrollmentService, EnrollmentService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICsvTransferService, CsvTransferService>();

            return services;
        }
    }
}
