using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SIMS.Domain.Abstractions;
using SIMS.Infrastructure.Data;
using SIMS.Infrastructure.Factories;
using SIMS.Infrastructure.Options;
using SIMS.Infrastructure.Seeding;
using SIMS.Infrastructure.Security;

namespace SIMS.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSimsInfrastructure(
            this IServiceCollection services,
            string dataDirectory,
            string connectionString)
        {
            ArgumentNullException.ThrowIfNull(services);

            if (string.IsNullOrWhiteSpace(dataDirectory))
            {
                throw new ArgumentException("Data directory is required.", nameof(dataDirectory));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string is required.", nameof(connectionString));
            }

            var absoluteDirectory = Path.GetFullPath(dataDirectory);
            Directory.CreateDirectory(absoluteDirectory);

            services.AddSingleton(new CsvOptions { DataDirectory = absoluteDirectory });

            services.AddDbContext<SimsDbContext>(options => options.UseSqlServer(connectionString));

            services.AddScoped<IStudentRepository, EfStudentRepository>();
            services.AddScoped<ICourseRepository, EfCourseRepository>();
            services.AddScoped<IEnrollmentRepository, EfEnrollmentRepository>();
            services.AddScoped<IUserRepository, EfUserRepository>();
            services.AddScoped<ITeacherRepository, EfTeacherRepository>();

            services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
            services.AddSingleton<IUserFactory, AppUserFactory>();

            services.AddScoped<CsvDataSeeder>();
            services.AddScoped<IDataSeeder>(sp => sp.GetRequiredService<CsvDataSeeder>());

            return services;
        }
    }
}
