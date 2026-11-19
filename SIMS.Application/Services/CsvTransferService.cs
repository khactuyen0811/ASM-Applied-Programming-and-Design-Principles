using System.Globalization;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using SIMS.Application.Abstractions;
using SIMS.Application.Dtos;
using SIMS.Application.Internal;
using SIMS.Domain.Abstractions;
using SIMS.Domain.Entities;
using SIMS.Domain.Enums;

namespace SIMS.Application.Services
{
    public sealed class CsvTransferService : ICsvTransferService
    {
        private const int MaxPreviewErrorMessages = 50;

        private static readonly string[] StudentRequiredHeaders =
        {
            "fullname",
            "dateofbirth",
            "gender",
            "email",
            "phone",
            "address",
            "program",
            "enrollmentyear"
        };

        private static readonly string[] CourseRequiredHeaders =
        {
            "coursecode",
            "name",
            "description",
            "credits",
            "capacity"
        };

        private readonly IStudentRepository _students;
        private readonly ICourseRepository _courses;
        private readonly ITeacherRepository _teachers;
        private readonly IEnrollmentRepository _enrollments;
        private readonly CsvTransferOptions _options;

        public CsvTransferService(
            IStudentRepository students,
            ICourseRepository courses,
            ITeacherRepository teachers,
            IEnrollmentRepository enrollments)
            : this(students, courses, teachers, enrollments, new CsvTransferOptions())
        {
        }

        public CsvTransferService(
            IStudentRepository students,
            ICourseRepository courses,
            ITeacherRepository teachers,
            IEnrollmentRepository enrollments,
            CsvTransferOptions options)
        {
            _students = students ?? throw new ArgumentNullException(nameof(students));
            _courses = courses ?? throw new ArgumentNullException(nameof(courses));
            _teachers = teachers ?? throw new ArgumentNullException(nameof(teachers));
            _enrollments = enrollments ?? throw new ArgumentNullException(nameof(enrollments));
            _options = options ?? new CsvTransferOptions();
        }

        public async Task<CsvPreview> PreviewStudentsAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var read = ReadRows(stream, StudentRequiredHeaders, _options.MaxRows, cancellationToken);
            if (read.Blocked)
            {
                return new CsvPreview(
                    CsvDatasetKind.Students,
                    read.Headers,
                    Array.Empty<CsvPreviewRow>(),
                    read.TotalRows,
                    read.Errors,
                    false);
            }

            var context = await CreateStudentContextAsync(cancellationToken).ConfigureAwait(false);
            var rows = new List<CsvPreviewRow>();
            var errors = new List<string>();

            foreach (var row in read.Rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var (_, rowErrors) = await ParseStudentAsync(row, context, cancellationToken).ConfigureAwait(false);
                var errorText = rowErrors.Count == 0 ? null : string.Join(" ", rowErrors);
                if (rows.Count < _options.MaxPreviewRows)
                {
                    rows.Add(new CsvPreviewRow(row.RowNumber, row.Values, errorText));
                }

                AddPreviewErrors(errors, row.RowNumber, rowErrors);
            }

            return new CsvPreview(
                CsvDatasetKind.Students,
                read.Headers,
                rows,
                read.TotalRows,
                errors,
                errors.Count == 0);
        }

        public async Task<CsvImportResult> ImportStudentsAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var read = ReadRows(stream, StudentRequiredHeaders, _options.MaxRows, cancellationToken);
            if (read.Blocked)
            {
                return CreateBlockedImportResult(CsvDatasetKind.Students, read);
            }

            var context = await CreateStudentContextAsync(cancellationToken).ConfigureAwait(false);
            var students = new List<Student>();
            var failures = new List<CsvImportError>();

            foreach (var row in read.Rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var (student, rowErrors) = await ParseStudentAsync(row, context, cancellationToken).ConfigureAwait(false);
                if (rowErrors.Count > 0)
                {
                    failures.Add(new CsvImportError(row.RowNumber, string.Join(" ", rowErrors)));
                    continue;
                }

                students.Add(student);
            }

            if (students.Count > 0)
            {
                await AddRangeBatchAsync(_students, students, cancellationToken).ConfigureAwait(false);
            }

            return new CsvImportResult(
                CsvDatasetKind.Students,
                read.TotalRows,
                students.Count,
                failures.Count,
                failures,
                failures.Count == 0 && students.Count > 0);
        }

        public async Task<CsvPreview> PreviewCoursesAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var read = ReadRows(stream, CourseRequiredHeaders, _options.MaxRows, cancellationToken);
            if (read.Blocked)
            {
                return new CsvPreview(
                    CsvDatasetKind.Courses,
                    read.Headers,
                    Array.Empty<CsvPreviewRow>(),
                    read.TotalRows,
                    read.Errors,
                    false);
            }

            var context = await CreateCourseContextAsync(cancellationToken).ConfigureAwait(false);
            var rows = new List<CsvPreviewRow>();
            var errors = new List<string>();

            foreach (var row in read.Rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var (_, rowErrors) = await ParseCourseAsync(row, context, cancellationToken).ConfigureAwait(false);
                var errorText = rowErrors.Count == 0 ? null : string.Join(" ", rowErrors);
                if (rows.Count < _options.MaxPreviewRows)
                {
                    rows.Add(new CsvPreviewRow(row.RowNumber, row.Values, errorText));
                }

                AddPreviewErrors(errors, row.RowNumber, rowErrors);
            }

            return new CsvPreview(
                CsvDatasetKind.Courses,
                read.Headers,
                rows,
                read.TotalRows,
                errors,
                errors.Count == 0);
        }

        public async Task<CsvImportResult> ImportCoursesAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var read = ReadRows(stream, CourseRequiredHeaders, _options.MaxRows, cancellationToken);
            if (read.Blocked)
            {
                return CreateBlockedImportResult(CsvDatasetKind.Courses, read);
            }

            var context = await CreateCourseContextAsync(cancellationToken).ConfigureAwait(false);
            var courses = new List<Course>();
            var failures = new List<CsvImportError>();

            foreach (var row in read.Rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var (course, rowErrors) = await ParseCourseAsync(row, context, cancellationToken).ConfigureAwait(false);
                if (rowErrors.Count > 0)
                {
                    failures.Add(new CsvImportError(row.RowNumber, string.Join(" ", rowErrors)));
                    continue;
                }

                courses.Add(course);
            }

            if (courses.Count > 0)
            {
                await AddRangeBatchAsync(_courses, courses, cancellationToken).ConfigureAwait(false);
            }

            return new CsvImportResult(
                CsvDatasetKind.Courses,
                read.TotalRows,
                courses.Count,
                failures.Count,
                failures,
                failures.Count == 0 && courses.Count > 0);
        }

        public async Task<CsvExportFile> ExportAsync(CsvDatasetKind kind, CancellationToken cancellationToken = default)
        {
            switch (kind)
            {
                case CsvDatasetKind.Students:
                    return await ExportStudentsAsync(cancellationToken).ConfigureAwait(false);
                case CsvDatasetKind.Courses:
                    return await ExportCoursesAsync(cancellationToken).ConfigureAwait(false);
                case CsvDatasetKind.Enrollments:
                    return await ExportEnrollmentsAsync(cancellationToken).ConfigureAwait(false);
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported dataset kind.");
            }
        }

        public CsvExportFile GetTemplate(CsvDatasetKind kind)
        {
            switch (kind)
            {
                case CsvDatasetKind.Students:
                    return BuildFile("students-template.csv", csv =>
                    {
                        WriteFields(csv, "StudentCode", "FullName", "DateOfBirth", "Gender", "Email", "Phone", "Address", "Program", "EnrollmentYear", "Status");
                        WriteFields(csv, "SE202600001", "Sample Student", "2005-01-01", "Other", "sample.student@example.com", "0900000000", "123 Sample Street", "Software Engineering", "2026", "Active");
                    });
                case CsvDatasetKind.Courses:
                    return BuildFile("courses-template.csv", csv =>
                    {
                        WriteFields(csv, "CourseCode", "Name", "Description", "Credits", "Capacity", "LecturerId");
                        WriteFields(csv, "CRS999", "Sample Course", "Sample course description.", "3", "30", string.Empty);
                    });
                case CsvDatasetKind.Enrollments:
                    return BuildFile("enrollments-template.csv", csv =>
                    {
                        WriteFields(csv, "StudentCode", "StudentName", "CourseCode", "CourseName", "Credits", "EnrolledAt", "Grade");
                        WriteFields(csv, "SE202600001", "Sample Student", "CRS999", "Sample Course", "3", "2026-01-01T00:00:00.0000000", "8.5");
                    });
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported dataset kind.");
            }
        }

        private async Task<CsvExportFile> ExportStudentsAsync(CancellationToken cancellationToken)
        {
            var students = await _students.GetAllAsync(cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            return BuildFile("students.csv", csv =>
            {
                WriteFields(csv, "StudentCode", "FullName", "DateOfBirth", "Gender", "Email", "Phone", "Address", "Program", "EnrollmentYear", "Status");
                foreach (var student in students.OrderBy(s => s.StudentCode, StringComparer.OrdinalIgnoreCase))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    WriteFields(
                        csv,
                        student.StudentCode,
                        student.FullName,
                        student.DateOfBirth.ToString("O", CultureInfo.InvariantCulture),
                        student.Gender,
                        student.Email,
                        student.Phone,
                        student.Address,
                        student.Program,
                        student.EnrollmentYear.ToString(CultureInfo.InvariantCulture),
                        student.Status.ToString());
                }
            });
        }

        private async Task<CsvExportFile> ExportCoursesAsync(CancellationToken cancellationToken)
        {
            var courses = await _courses.GetAllAsync(cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            return BuildFile("courses.csv", csv =>
            {
                WriteFields(csv, "CourseCode", "Name", "Description", "Credits", "Capacity", "LecturerId");
                foreach (var course in courses.OrderBy(c => c.CourseCode, StringComparer.OrdinalIgnoreCase))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    WriteFields(
                        csv,
                        course.CourseCode,
                        course.Name,
                        course.Description,
                        course.Credits.ToString(CultureInfo.InvariantCulture),
                        course.Capacity.ToString(CultureInfo.InvariantCulture),
                        course.LecturerId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
                }
            });
        }

        private async Task<CsvExportFile> ExportEnrollmentsAsync(CancellationToken cancellationToken)
        {
            var students = await _students.GetAllAsync(cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            var courses = await _courses.GetAllAsync(cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            var enrollments = await _enrollments.GetAllAsync(cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            var studentsById = new Dictionary<int, Student>();
            foreach (var student in students)
            {
                studentsById[student.Id] = student;
            }

            var coursesById = new Dictionary<int, Course>();
            foreach (var course in courses)
            {
                coursesById[course.Id] = course;
            }

            return BuildFile("enrollments.csv", csv =>
            {
                WriteFields(csv, "StudentCode", "StudentName", "CourseCode", "CourseName", "Credits", "EnrolledAt", "Grade");
                foreach (var enrollment in enrollments.OrderBy(e => e.EnrolledAt).ThenBy(e => e.Id))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    studentsById.TryGetValue(enrollment.StudentId, out var student);
                    coursesById.TryGetValue(enrollment.CourseId, out var course);

                    WriteFields(
                        csv,
                        student?.StudentCode ?? string.Empty,
                        student?.FullName ?? string.Empty,
                        course?.CourseCode ?? string.Empty,
                        course?.Name ?? string.Empty,
                        course is null ? string.Empty : course.Credits.ToString(CultureInfo.InvariantCulture),
                        enrollment.EnrolledAt.ToString("O", CultureInfo.InvariantCulture),
                        enrollment.Grade?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
                }
            });
        }

        private async Task<(Student Student, List<string> Errors)> ParseStudentAsync(CsvRow row, StudentContext context, CancellationToken cancellationToken)
        {
            var errors = new List<string>();

            var fullName = GetField(row, "fullname");
            var dateOfBirthValue = GetField(row, "dateofbirth");
            var dateOfBirth = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(dateOfBirthValue))
            {
                errors.Add("Date of birth is required.");
            }
            else if (!TryParseDate(dateOfBirthValue, out dateOfBirth))
            {
                errors.Add("Date of birth is not a valid date.");
                dateOfBirth = DateTime.MinValue;
            }

            var genderValue = GetField(row, "gender");
            var gender = "Other";
            if (!string.IsNullOrWhiteSpace(genderValue) && !TryParseGender(genderValue, out gender))
            {
                errors.Add("Gender must be Male, Female, or Other.");
            }

            var email = GetField(row, "email");
            var emailValid = IsValidEmail(email);
            if (!string.IsNullOrWhiteSpace(email) && !emailValid)
            {
                errors.Add("Email is not a valid email address.");
            }

            var phone = GetField(row, "phone");
            var address = GetField(row, "address");
            var program = GetField(row, "program");

            var enrollmentYearValue = GetField(row, "enrollmentyear");
            var enrollmentYear = DateTime.UtcNow.Year;
            var enrollmentYearParsed = false;
            if (string.IsNullOrWhiteSpace(enrollmentYearValue))
            {
                errors.Add("Enrollment year is required.");
            }
            else if (!TryParseInt(enrollmentYearValue, out enrollmentYear))
            {
                errors.Add("Enrollment year must be a number between 2000 and 2100.");
                enrollmentYear = DateTime.UtcNow.Year;
            }
            else
            {
                enrollmentYearParsed = true;
            }

            var statusValue = GetField(row, "status");
            var status = StudentStatus.Active;
            if (!string.IsNullOrWhiteSpace(statusValue) && !TryParseStatus(statusValue, out status))
            {
                errors.Add("Status must be Active, Suspended, or Graduated.");
            }

            var studentCode = GetField(row, "studentcode");
            if (!string.IsNullOrWhiteSpace(studentCode))
            {
                if (!context.FileCodes.Add(studentCode))
                {
                    errors.Add("Student code is duplicated in the file.");
                }

                if (context.ExistingCodes.Contains(studentCode)
                    || await _students.GetByCodeAsync(studentCode, cancellationToken).ConfigureAwait(false) is not null)
                {
                    errors.Add("Student code already exists.");
                }
            }
            else if (enrollmentYearParsed && enrollmentYear >= 2000 && enrollmentYear <= 2100)
            {
                studentCode = await GenerateStudentCodeAsync(enrollmentYear, context, cancellationToken).ConfigureAwait(false);
                context.FileCodes.Add(studentCode);
            }
            else
            {
                studentCode = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                if (!context.FileEmails.Add(email))
                {
                    errors.Add("Email is duplicated in the file.");
                }

                if (emailValid && await _students.EmailExistsAsync(email, null, cancellationToken).ConfigureAwait(false))
                {
                    errors.Add("Email is already in use by another student.");
                }
            }

            var student = new Student
            {
                StudentCode = studentCode,
                FullName = fullName,
                DateOfBirth = dateOfBirth,
                Gender = gender,
                Email = email,
                Phone = phone,
                Address = address,
                Program = program,
                EnrollmentYear = enrollmentYear,
                Status = status,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var validationError in ValidationHelper.Validate(student))
            {
                if (!errors.Contains(validationError))
                {
                    errors.Add(validationError);
                }
            }

            return (student, errors);
        }

        private async Task<(Course Course, List<string> Errors)> ParseCourseAsync(CsvRow row, CourseContext context, CancellationToken cancellationToken)
        {
            var errors = new List<string>();

            var courseCode = GetField(row, "coursecode");
            if (!string.IsNullOrWhiteSpace(courseCode))
            {
                if (!context.FileCodes.Add(courseCode))
                {
                    errors.Add("Course code is duplicated in the file.");
                }

                if (await _courses.GetByCodeAsync(courseCode, cancellationToken).ConfigureAwait(false) is not null)
                {
                    errors.Add("Course code already exists.");
                }
            }

            var name = GetField(row, "name");
            var description = GetField(row, "description");

            var creditsValue = GetField(row, "credits");
            var credits = 3;
            if (string.IsNullOrWhiteSpace(creditsValue))
            {
                errors.Add("Credits is required.");
            }
            else if (!TryParseInt(creditsValue, out credits))
            {
                errors.Add("Credits must be a number between 1 and 60.");
                credits = 3;
            }

            var capacityValue = GetField(row, "capacity");
            var capacity = 30;
            if (string.IsNullOrWhiteSpace(capacityValue))
            {
                errors.Add("Capacity is required.");
            }
            else if (!TryParseInt(capacityValue, out capacity))
            {
                errors.Add("Capacity must be a number between 1 and 1000.");
                capacity = 30;
            }

            var lecturerId = await ResolveLecturerAsync(row, context, errors, cancellationToken).ConfigureAwait(false);

            var course = new Course
            {
                CourseCode = courseCode,
                Name = name,
                Description = description,
                Credits = credits,
                Capacity = capacity,
                LecturerId = lecturerId,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var validationError in ValidationHelper.Validate(course))
            {
                if (!errors.Contains(validationError))
                {
                    errors.Add(validationError);
                }
            }

            return (course, errors);
        }

        private async Task<int?> ResolveLecturerAsync(CsvRow row, CourseContext context, List<string> errors, CancellationToken cancellationToken)
        {
            var lecturerIdValue = GetField(row, "lecturerid");
            if (!string.IsNullOrWhiteSpace(lecturerIdValue))
            {
                if (!TryParseInt(lecturerIdValue, out var lecturerId))
                {
                    errors.Add("Lecturer id must be a number.");
                    return null;
                }

                var teacher = await _teachers.GetByIdAsync(lecturerId, cancellationToken).ConfigureAwait(false);
                if (teacher is null)
                {
                    errors.Add("Lecturer does not exist.");
                    return null;
                }

                return teacher.Id;
            }

            var lecturerCode = GetField(row, "lecturercode");
            if (!string.IsNullOrWhiteSpace(lecturerCode))
            {
                if (context.TeachersByCode.TryGetValue(lecturerCode, out var byCode))
                {
                    return byCode.Id;
                }

                errors.Add($"Lecturer code '{lecturerCode}' was not found.");
                return null;
            }

            var lecturerEmail = GetField(row, "lectureremail");
            if (!string.IsNullOrWhiteSpace(lecturerEmail))
            {
                if (context.TeachersByEmail.TryGetValue(lecturerEmail, out var byEmail))
                {
                    return byEmail.Id;
                }

                errors.Add($"Lecturer email '{lecturerEmail}' was not found.");
                return null;
            }

            return null;
        }

        private async Task<StudentContext> CreateStudentContextAsync(CancellationToken cancellationToken)
        {
            var context = new StudentContext();
            var existing = await _students.GetAllAsync(cancellationToken).ConfigureAwait(false);

            foreach (var student in existing)
            {
                var code = student.StudentCode;
                if (string.IsNullOrWhiteSpace(code))
                {
                    continue;
                }

                context.ExistingCodes.Add(code);
                if (code.Length > 6
                    && code.StartsWith("SE", StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(code.Substring(2, 4), NumberStyles.None, CultureInfo.InvariantCulture, out var year)
                    && int.TryParse(code.Substring(6), NumberStyles.None, CultureInfo.InvariantCulture, out var sequence))
                {
                    if (!context.MaxSequenceByYear.TryGetValue(year, out var max) || sequence > max)
                    {
                        context.MaxSequenceByYear[year] = sequence;
                    }
                }
            }

            return context;
        }

        private async Task<CourseContext> CreateCourseContextAsync(CancellationToken cancellationToken)
        {
            var context = new CourseContext();
            var teachers = await _teachers.GetAllAsync(cancellationToken).ConfigureAwait(false);

            foreach (var teacher in teachers)
            {
                if (!string.IsNullOrWhiteSpace(teacher.TeacherCode))
                {
                    context.TeachersByCode.TryAdd(teacher.TeacherCode, teacher);
                }

                if (!string.IsNullOrWhiteSpace(teacher.Email))
                {
                    context.TeachersByEmail.TryAdd(teacher.Email, teacher);
                }
            }

            return context;
        }

        private async Task<string> GenerateStudentCodeAsync(int enrollmentYear, StudentContext context, CancellationToken cancellationToken)
        {
            context.MaxSequenceByYear.TryGetValue(enrollmentYear, out var sequence);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                sequence++;
                var candidate = $"SE{enrollmentYear}{sequence:D5}";
                if (!context.FileCodes.Contains(candidate)
                    && !context.ExistingCodes.Contains(candidate)
                    && await _students.GetByCodeAsync(candidate, cancellationToken).ConfigureAwait(false) is null)
                {
                    context.MaxSequenceByYear[enrollmentYear] = sequence;
                    return candidate;
                }
            }
        }

        private static void AddPreviewErrors(List<string> errors, int rowNumber, IReadOnlyList<string> rowErrors)
        {
            foreach (var message in rowErrors)
            {
                if (errors.Count >= MaxPreviewErrorMessages)
                {
                    return;
                }

                errors.Add($"Row {rowNumber}: {message}");
            }
        }

        private static CsvImportResult CreateBlockedImportResult(CsvDatasetKind kind, CsvReadResult read)
        {
            var errors = new List<CsvImportError>(read.Errors.Count);
            foreach (var message in read.Errors)
            {
                errors.Add(new CsvImportError(read.ErrorRowNumber, message));
            }

            return new CsvImportResult(kind, read.TotalRows, 0, 0, errors, false);
        }

        private static async Task AddRangeBatchAsync<T>(IRepository<T> repository, IReadOnlyList<T> items, CancellationToken cancellationToken)
            where T : class
        {
            if (items.Count == 0)
            {
                return;
            }

            var method = repository.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "AddRangeAsync" && m.GetParameters().Length == 2);

            if (method is not null)
            {
                var result = method.Invoke(repository, new object[] { items, cancellationToken });
                if (result is Task task)
                {
                    await task.ConfigureAwait(false);
                    return;
                }
            }

            foreach (var item in items)
            {
                await repository.AddAsync(item, cancellationToken).ConfigureAwait(false);
            }
        }

        private static CsvReadResult ReadRows(
            Stream stream,
            IReadOnlyList<string> requiredHeaders,
            int maxRows,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(stream);
            cancellationToken.ThrowIfCancellationRequested();

            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            var rows = new List<CsvRow>();
            var headers = Array.Empty<string>();

            using var reader = new StreamReader(stream, new UTF8Encoding(false), true, 4096, true);
            using var csv = new CsvReader(reader, CreateConfiguration());

            try
            {
                if (!csv.Read())
                {
                    return BlockedResult(Array.Empty<string>(), 1, "The file is empty or missing a header row.");
                }

                csv.ReadHeader();
                headers = (csv.HeaderRecord ?? Array.Empty<string>())
                    .Select(header => header?.Trim() ?? string.Empty)
                    .ToArray();
            }
            catch (CsvHelperException ex)
            {
                return BlockedResult(headers, 1, $"The CSV header could not be read: {ex.Message}");
            }

            if (headers.Length == 0)
            {
                return BlockedResult(headers, 1, "The file is missing a header row.");
            }

            var headerIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Length; i++)
            {
                var key = headers[i].Trim().ToLowerInvariant();
                if (key.Length == 0 || headerIndex.ContainsKey(key))
                {
                    continue;
                }

                headerIndex[key] = i;
            }

            var missing = new List<string>();
            foreach (var header in requiredHeaders)
            {
                if (!headerIndex.ContainsKey(header))
                {
                    missing.Add(header);
                }
            }

            if (missing.Count > 0)
            {
                var errors = new List<string>(missing.Count);
                foreach (var header in missing)
                {
                    errors.Add($"Missing required header: {header}.");
                }

                return new CsvReadResult
                {
                    Headers = headers,
                    Rows = Array.Empty<CsvRow>(),
                    Errors = errors,
                    TotalRows = 0,
                    ErrorRowNumber = 1,
                    Blocked = true
                };
            }

            var rowNumber = 1;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                bool hasRow;
                try
                {
                    hasRow = csv.Read();
                }
                catch (CsvHelperException ex)
                {
                    return new CsvReadResult
                    {
                        Headers = headers,
                        Rows = rows,
                        Errors = new[] { $"The CSV could not be read: {ex.Message}" },
                        TotalRows = rows.Count,
                        ErrorRowNumber = rowNumber + 1,
                        Blocked = true
                    };
                }

                if (!hasRow)
                {
                    break;
                }

                rowNumber++;
                var values = new string[headers.Length];
                var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                for (var i = 0; i < headers.Length; i++)
                {
                    string value;
                    try
                    {
                        value = (csv.GetField(i) ?? string.Empty).Trim();
                    }
                    catch (CsvHelperException)
                    {
                        value = string.Empty;
                    }

                    values[i] = value;
                    var key = headers[i].Trim().ToLowerInvariant();
                    if (key.Length > 0)
                    {
                        fields[key] = value;
                    }
                }

                rows.Add(new CsvRow
                {
                    RowNumber = rowNumber,
                    Values = values,
                    Fields = fields
                });

                if (rows.Count > maxRows)
                {
                    return new CsvReadResult
                    {
                        Headers = headers,
                        Rows = rows,
                        Errors = new[] { $"The file exceeds the maximum of {maxRows} rows." },
                        TotalRows = rows.Count,
                        ErrorRowNumber = 0,
                        Blocked = true
                    };
                }
            }

            return new CsvReadResult
            {
                Headers = headers,
                Rows = rows,
                Errors = Array.Empty<string>(),
                TotalRows = rows.Count,
                ErrorRowNumber = 0,
                Blocked = false
            };
        }

        private static CsvReadResult BlockedResult(IReadOnlyList<string> headers, int rowNumber, string message)
        {
            return new CsvReadResult
            {
                Headers = headers,
                Rows = Array.Empty<CsvRow>(),
                Errors = new[] { message },
                TotalRows = 0,
                ErrorRowNumber = rowNumber,
                Blocked = true
            };
        }

        private static CsvConfiguration CreateConfiguration()
        {
            return new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant(),
                HasHeaderRecord = true,
                BadDataFound = null,
                MissingFieldFound = null,
                HeaderValidated = null,
                TrimOptions = TrimOptions.Trim,
                DetectColumnCountChanges = true
            };
        }

        private static CsvExportFile BuildFile(string fileName, Action<CsvWriter> write)
        {
            byte[] content;
            using (var memory = new MemoryStream())
            {
                using (var writer = new StreamWriter(memory, new UTF8Encoding(true), 4096, true))
                using (var csv = new CsvWriter(writer, CreateConfiguration()))
                {
                    write(csv);
                }

                content = memory.ToArray();
            }

            return new CsvExportFile(fileName, "text/csv", content);
        }

        private static void WriteFields(CsvWriter csv, params string[] values)
        {
            foreach (var value in values)
            {
                csv.WriteField(value ?? string.Empty);
            }

            csv.NextRecord();
        }

        private static string GetField(CsvRow row, string name)
        {
            return row.Fields.TryGetValue(name, out var value) ? value : string.Empty;
        }

        private static bool TryParseDate(string value, out DateTime result)
        {
            return DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out result);
        }

        private static bool TryParseInt(string value, out int result)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        private static bool TryParseGender(string value, out string gender)
        {
            switch (value.Trim().ToLowerInvariant())
            {
                case "male":
                    gender = "Male";
                    return true;
                case "female":
                    gender = "Female";
                    return true;
                case "other":
                    gender = "Other";
                    return true;
                default:
                    gender = "Other";
                    return false;
            }
        }

        private static bool TryParseStatus(string value, out StudentStatus status)
        {
            switch (value.Trim().ToLowerInvariant())
            {
                case "active":
                    status = StudentStatus.Active;
                    return true;
                case "suspended":
                    status = StudentStatus.Suspended;
                    return true;
                case "graduated":
                    status = StudentStatus.Graduated;
                    return true;
                default:
                    status = StudentStatus.Active;
                    return false;
            }
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            try
            {
                var address = new MailAddress(email);
                return string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private sealed class CsvRow
        {
            public int RowNumber { get; init; }

            public IReadOnlyList<string> Values { get; init; } = Array.Empty<string>();

            public IReadOnlyDictionary<string, string> Fields { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        private sealed class CsvReadResult
        {
            public IReadOnlyList<string> Headers { get; init; } = Array.Empty<string>();

            public IReadOnlyList<CsvRow> Rows { get; init; } = Array.Empty<CsvRow>();

            public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

            public int TotalRows { get; init; }

            public int ErrorRowNumber { get; init; }

            public bool Blocked { get; init; }
        }

        private sealed class StudentContext
        {
            public HashSet<string> ExistingCodes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public HashSet<string> FileCodes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public HashSet<string> FileEmails { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public Dictionary<int, int> MaxSequenceByYear { get; } = new Dictionary<int, int>();
        }

        private sealed class CourseContext
        {
            public HashSet<string> FileCodes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public Dictionary<string, Teacher> TeachersByCode { get; } = new Dictionary<string, Teacher>(StringComparer.OrdinalIgnoreCase);

            public Dictionary<string, Teacher> TeachersByEmail { get; } = new Dictionary<string, Teacher>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
