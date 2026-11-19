using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SIMS.Application;
using SIMS.Domain.Abstractions;
using SIMS.Infrastructure;
using SIMS.Infrastructure.Data;

namespace SIMS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            var dataDirectory = builder.Configuration["Sims:DataDirectory"] ?? "App_Data";
            if (!Path.IsPathRooted(dataDirectory))
            {
                dataDirectory = Path.Combine(builder.Environment.ContentRootPath, dataDirectory);
            }

            var connectionString = builder.Configuration.GetConnectionString("SimsDb")
                ?? "Server=(localdb)\\MSSQLLocalDB;Database=SIMS;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

            builder.Services.AddSimsApplication();
            builder.Services.AddSimsInfrastructure(dataDirectory, connectionString);

            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login";
                    options.LogoutPath = "/Login/Logout";
                    options.AccessDeniedPath = "/Home/Denied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                    options.Cookie.Name = "SIMS.Auth";
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("StaffOnly", policy => policy.RequireRole("Admin", "Lecturer"));
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Login}/{action=Index}/{id?}");

            using (var scope = app.Services.CreateScope())
            {
                var database = scope.ServiceProvider.GetRequiredService<SimsDbContext>();
                database.Database.EnsureCreated();

                var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
                seeder.SeedAsync().GetAwaiter().GetResult();
            }

            app.Run();
        }
    }
}
