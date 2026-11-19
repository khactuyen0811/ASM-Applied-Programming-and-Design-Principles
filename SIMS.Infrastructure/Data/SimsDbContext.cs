using Microsoft.EntityFrameworkCore;
using SIMS.Domain.Entities;

namespace SIMS.Infrastructure.Data
{
    public sealed class SimsDbContext : DbContext
    {
        public SimsDbContext(DbContextOptions<SimsDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students => Set<Student>();

        public DbSet<Teacher> Teachers => Set<Teacher>();

        public DbSet<Course> Courses => Set<Course>();

        public DbSet<Enrollment> Enrollments => Set<Enrollment>();

        public DbSet<AppUser> Users => Set<AppUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("Students");
                entity.HasKey(student => student.Id);
                entity.Property(student => student.StudentCode).HasMaxLength(20).IsRequired();
                entity.Property(student => student.FullName).HasMaxLength(120).IsRequired();
                entity.Property(student => student.Gender).HasMaxLength(10).IsRequired();
                entity.Property(student => student.Email).HasMaxLength(160).IsRequired();
                entity.Property(student => student.Phone).HasMaxLength(20);
                entity.Property(student => student.Address).HasMaxLength(250);
                entity.Property(student => student.Program).HasMaxLength(120).IsRequired();
                entity.HasIndex(student => student.StudentCode).IsUnique();
                entity.HasIndex(student => student.Email).IsUnique();
            });

            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.ToTable("Teachers");
                entity.HasKey(teacher => teacher.Id);
                entity.Property(teacher => teacher.TeacherCode).HasMaxLength(20).IsRequired();
                entity.Property(teacher => teacher.FullName).HasMaxLength(120).IsRequired();
                entity.Property(teacher => teacher.Email).HasMaxLength(160).IsRequired();
                entity.Property(teacher => teacher.Phone).HasMaxLength(20);
                entity.Property(teacher => teacher.Department).HasMaxLength(120).IsRequired();
                entity.HasIndex(teacher => teacher.TeacherCode).IsUnique();
                entity.HasIndex(teacher => teacher.Email).IsUnique();
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("Courses");
                entity.HasKey(course => course.Id);
                entity.Property(course => course.CourseCode).HasMaxLength(20).IsRequired();
                entity.Property(course => course.Name).HasMaxLength(150).IsRequired();
                entity.Property(course => course.Description).HasMaxLength(500);
                entity.HasIndex(course => course.CourseCode).IsUnique();
                entity.HasIndex(course => course.LecturerId);
            });

            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.ToTable("Enrollments");
                entity.HasKey(enrollment => enrollment.Id);
                entity.HasIndex(enrollment => new { enrollment.StudentId, enrollment.CourseId }).IsUnique();
                entity.HasIndex(enrollment => enrollment.CourseId);
            });

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(user => user.Id);
                entity.Property(user => user.Username).HasMaxLength(60).IsRequired();
                entity.Property(user => user.DisplayName).HasMaxLength(120).IsRequired();
                entity.Property(user => user.PasswordHash).IsRequired();
                entity.Property(user => user.PasswordSalt).IsRequired();
                entity.HasIndex(user => user.Username).IsUnique();
                entity.HasIndex(user => user.StudentId);
                entity.HasIndex(user => user.TeacherId);
            });
        }
    }
}
