using SIMS.Domain.Abstractions;
using SIMS.Domain.Entities;
using SIMS.Domain.Enums;
using SIMS.Infrastructure.Csv;

namespace SIMS.Infrastructure.Seeding
{
    public sealed class CsvDataSeeder : IDataSeeder
    {
        private const int TeacherCount = 120;
        private const int CourseCount = 60;
        private const int StudentCount = 10_000;

        private static readonly string[] Ho =
        {
            "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Huỳnh", "Phan", "Vũ", "Võ", "Đặng",
            "Bùi", "Đỗ", "Hồ", "Ngô", "Dương", "Lý", "Đinh", "Trịnh", "Mai", "Tô",
            "Lương", "Đào", "Đoàn", "Vương", "Lâm"
        };

        private static readonly string[] Ten =
        {
            "An", "Bình", "Chi", "Dũng", "Dương", "Giang", "Hà", "Hải", "Hiếu", "Hoa",
            "Hùng", "Huy", "Hương", "Khánh", "Lan", "Linh", "Long", "Mai", "Minh", "Nam",
            "Ngọc", "Nhung", "Phúc", "Quân", "Quỳnh", "Sơn", "Tâm", "Thảo", "Trang", "Tuấn"
        };

        private static readonly string[] Departments =
        {
            "Computing", "Business", "Mathematics", "Languages", "Design", "Engineering"
        };

        private static readonly string[] Programs =
        {
            "Software Engineering", "Information Systems", "Computer Science",
            "Information Security", "Artificial Intelligence", "Data Science"
        };

        private static readonly string[] Cities =
        {
            "Ha Noi", "Ho Chi Minh City", "Da Nang"
        };

        private static readonly string[] CourseNames =
        {
            "Programming Fundamentals", "Database Systems", "Data Structures and Algorithms",
            "Object-Oriented Programming", "Computer Networks", "Operating Systems",
            "Software Engineering", "Web Application Development", "Mobile Application Development",
            "Discrete Mathematics", "Linear Algebra", "Calculus I",
            "Calculus II", "Probability and Statistics", "Introduction to Information Systems",
            "Systems Analysis and Design", "Database Administration", "Cloud Computing",
            "Cybersecurity Fundamentals", "Network Security", "Cryptography",
            "Artificial Intelligence", "Machine Learning", "Deep Learning",
            "Natural Language Processing", "Computer Vision", "Data Mining",
            "Big Data Analytics", "Data Visualization", "Business Intelligence",
            "Principles of Management", "Marketing Management", "Financial Accounting",
            "Managerial Accounting", "Business Communication", "Organizational Behavior",
            "Human Resource Management", "Operations Management", "Project Management",
            "Software Testing", "Software Architecture", "DevOps Engineering",
            "Distributed Systems", "Parallel Computing", "Compiler Design",
            "Theory of Computation", "Computer Graphics", "Game Development",
            "Human-Computer Interaction", "Digital Logic Design", "Computer Organization",
            "Embedded Systems", "Internet of Things", "Blockchain Technology",
            "E-Commerce Systems", "Enterprise Resource Planning", "Customer Relationship Management",
            "Supply Chain Management", "Entrepreneurship", "Innovation Management"
        };

        private readonly IStudentRepository _students;
        private readonly ICourseRepository _courses;
        private readonly IEnrollmentRepository _enrollments;
        private readonly ITeacherRepository _teachers;
        private readonly IUserRepository _users;
        private readonly IUserFactory _userFactory;

        public CsvDataSeeder(
            IStudentRepository students,
            ICourseRepository courses,
            IEnrollmentRepository enrollments,
            ITeacherRepository teachers,
            IUserRepository users,
            IUserFactory userFactory)
        {
            _students = students ?? throw new ArgumentNullException(nameof(students));
            _courses = courses ?? throw new ArgumentNullException(nameof(courses));
            _enrollments = enrollments ?? throw new ArgumentNullException(nameof(enrollments));
            _teachers = teachers ?? throw new ArgumentNullException(nameof(teachers));
            _users = users ?? throw new ArgumentNullException(nameof(users));
            _userFactory = userFactory ?? throw new ArgumentNullException(nameof(userFactory));
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            await SeedTeachersAsync(cancellationToken).ConfigureAwait(false);

            var teachers = await _teachers.GetAllAsync(cancellationToken).ConfigureAwait(false);
            await SeedCoursesAsync(teachers, cancellationToken).ConfigureAwait(false);
            await SeedStudentsAsync(cancellationToken).ConfigureAwait(false);
            await SeedUsersAsync(cancellationToken).ConfigureAwait(false);
            await SeedEnrollmentsAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task SeedTeachersAsync(CancellationToken cancellationToken)
        {
            if (await _teachers.CountAsync(cancellationToken).ConfigureAwait(false) > 0)
            {
                return;
            }

            var teachers = new List<Teacher>(TeacherCount);
            for (var i = 0; i < TeacherCount; i++)
            {
                var code = $"GV{i + 1:D4}";
                teachers.Add(new Teacher
                {
                    TeacherCode = code,
                    FullName = $"{Ho[i % Ho.Length]} {Ten[(i * 7) % Ten.Length]}",
                    Email = $"{code}@sims.edu.vn",
                    Phone = $"09{i % 100_000_000:D8}",
                    Department = Departments[i % Departments.Length],
                    CreatedAt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });
            }

            await AddManyAsync(_teachers, teachers, cancellationToken).ConfigureAwait(false);
        }

        private async Task SeedCoursesAsync(IReadOnlyList<Teacher> teachers, CancellationToken cancellationToken)
        {
            if (await _courses.CountAsync(cancellationToken).ConfigureAwait(false) > 0)
            {
                return;
            }

            var courses = new List<Course>(CourseCount);
            for (var i = 0; i < CourseCount; i++)
            {
                int? lecturerId = null;
                if (teachers.Count > 0)
                {
                    lecturerId = teachers[i % teachers.Count].Id;
                }

                courses.Add(new Course
                {
                    CourseCode = $"CRS{i + 1:D3}",
                    Name = CourseNames[i],
                    Description = $"Core course covering {CourseNames[i]}.",
                    Credits = 3 + (i % 2),
                    Capacity = 30 + (i % 31),
                    LecturerId = lecturerId,
                    CreatedAt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });
            }

            await AddManyAsync(_courses, courses, cancellationToken).ConfigureAwait(false);
        }

        private async Task SeedStudentsAsync(CancellationToken cancellationToken)
        {
            if (await _students.CountAsync(cancellationToken).ConfigureAwait(false) > 0)
            {
                return;
            }

            var random = new Random(20250501);
            var students = new List<Student>(StudentCount);

            for (var i = 0; i < StudentCount; i++)
            {
                var enrollmentYear = 2022 + random.Next(0, 4);
                var statusRoll = random.Next(100);
                var status = statusRoll < 5
                    ? StudentStatus.Graduated
                    : statusRoll < 7
                        ? StudentStatus.Suspended
                        : StudentStatus.Active;

                var dateOfBirth = new DateTime(
                    2002 + random.Next(0, 5),
                    random.Next(1, 13),
                    random.Next(1, 29),
                    0,
                    0,
                    0,
                    DateTimeKind.Utc);

                var code = $"SE2024{i + 1:D5}";

                students.Add(new Student
                {
                    StudentCode = code,
                    FullName = $"{Ho[i % Ho.Length]} {Ten[(i * 11) % Ten.Length]}",
                    DateOfBirth = dateOfBirth,
                    Gender = random.Next(2) == 0 ? "Male" : "Female",
                    Email = $"{code}@student.sims.edu.vn",
                    Phone = $"09{i % 100_000_000:D8}",
                    Address = $"{random.Next(1, 500)} {Cities[i % Cities.Length]}",
                    Program = Programs[i % Programs.Length],
                    EnrollmentYear = enrollmentYear,
                    Status = status,
                    CreatedAt = new DateTime(enrollmentYear, 9, 1, 0, 0, 0, DateTimeKind.Utc)
                });
            }

            await AddManyAsync(_students, students, cancellationToken).ConfigureAwait(false);
        }

        private async Task SeedUsersAsync(CancellationToken cancellationToken)
        {
            if (await _users.CountAsync(cancellationToken).ConfigureAwait(false) > 0)
            {
                return;
            }

            var teachers = await _teachers.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var students = await _students.GetAllAsync(cancellationToken).ConfigureAwait(false);

            int? TeacherIdAt(int index) => index < teachers.Count ? teachers[index].Id : null;

            int? StudentIdAt(int index) => index < students.Count ? students[index].Id : null;

            var users = new List<AppUser>
            {
                _userFactory.Create("trieunt", "12345", "Thanh Trieu", UserRole.Admin),
                _userFactory.Create("lecturer", "12345", "Lecturer 1", UserRole.Lecturer, teacherId: TeacherIdAt(0)),
                _userFactory.Create("student", "12345", "Student 1", UserRole.Student, studentId: StudentIdAt(0))
            };

            for (var i = 2; i <= 4; i++)
            {
                users.Add(_userFactory.Create($"lecturer{i}", "12345", $"Lecturer {i}", UserRole.Lecturer, teacherId: TeacherIdAt(i - 1)));
                users.Add(_userFactory.Create($"student{i}", "12345", $"Student {i}", UserRole.Student, studentId: StudentIdAt(i - 1)));
            }

            await AddManyAsync(_users, users, cancellationToken).ConfigureAwait(false);
        }

        private async Task SeedEnrollmentsAsync(CancellationToken cancellationToken)
        {
            if (await _enrollments.CountAsync(cancellationToken).ConfigureAwait(false) > 0)
            {
                return;
            }

            var students = await _students.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var courses = await _courses.GetAllAsync(cancellationToken).ConfigureAwait(false);

            if (students.Count == 0 || courses.Count == 0)
            {
                return;
            }

            var random = new Random(20250502);
            var enrollments = new List<Enrollment>(students.Count * 3);

            foreach (var student in students)
            {
                var target = Math.Min(random.Next(1, 7), courses.Count);
                var selected = new HashSet<int>(target);

                while (selected.Count < target)
                {
                    var course = courses[random.Next(courses.Count)];
                    if (!selected.Add(course.Id))
                    {
                        continue;
                    }

                    double? grade = null;
                    if (random.Next(100) < 30)
                    {
                        grade = Math.Round(random.NextDouble() * 10.0, 1);
                    }

                    enrollments.Add(new Enrollment
                    {
                        StudentId = student.Id,
                        CourseId = course.Id,
                        EnrolledAt = new DateTime(
                            2022 + random.Next(0, 4),
                            random.Next(1, 13),
                            random.Next(1, 29),
                            0,
                            0,
                            0,
                            DateTimeKind.Utc),
                        Grade = grade
                    });
                }
            }

            await AddManyAsync(_enrollments, enrollments, cancellationToken).ConfigureAwait(false);
        }

        private static async Task AddManyAsync<T>(IRepository<T> repository, IReadOnlyList<T> items, CancellationToken cancellationToken)
            where T : class, new()
        {
            if (items.Count == 0)
            {
                return;
            }

            if (repository is IRangeRepository<T> rangeRepository)
            {
                await rangeRepository.AddRangeAsync(items, cancellationToken).ConfigureAwait(false);
                return;
            }

            foreach (var item in items)
            {
                await repository.AddAsync(item, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
