using SIMS.Domain.Enums;

namespace SIMS.Application.Dtos
{
    public sealed class StudentListItem
    {
        public StudentListItem()
        {
        }

        public StudentListItem(int id, string studentCode, string fullName, string email, string program, int enrollmentYear, StudentStatus status)
        {
            Id = id;
            StudentCode = studentCode;
            FullName = fullName;
            Email = email;
            Program = program;
            EnrollmentYear = enrollmentYear;
            Status = status;
        }

        public int Id { get; set; }

        public string StudentCode { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Program { get; set; } = string.Empty;

        public int EnrollmentYear { get; set; }

        public StudentStatus Status { get; set; }
    }

    public sealed class StudentDetails
    {
        public StudentDetails()
        {
        }

        public StudentDetails(
            int id,
            string studentCode,
            string fullName,
            DateTime dateOfBirth,
            string gender,
            string email,
            string phone,
            string address,
            string program,
            int enrollmentYear,
            StudentStatus status,
            DateTime createdAt,
            IReadOnlyList<EnrollmentListItem> enrollments)
        {
            Id = id;
            StudentCode = studentCode;
            FullName = fullName;
            DateOfBirth = dateOfBirth;
            Gender = gender;
            Email = email;
            Phone = phone;
            Address = address;
            Program = program;
            EnrollmentYear = enrollmentYear;
            Status = status;
            CreatedAt = createdAt;
            Enrollments = enrollments ?? Array.Empty<EnrollmentListItem>();
        }

        public int Id { get; set; }

        public string StudentCode { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public string Gender { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string Program { get; set; } = string.Empty;

        public int EnrollmentYear { get; set; }

        public StudentStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public IReadOnlyList<EnrollmentListItem> Enrollments { get; set; } = Array.Empty<EnrollmentListItem>();
    }

    public sealed class StudentCreateRequest
    {
        public StudentCreateRequest()
        {
        }

        public StudentCreateRequest(
            string fullName,
            DateTime dateOfBirth,
            string gender,
            string email,
            string phone,
            string address,
            string program,
            int enrollmentYear)
        {
            FullName = fullName;
            DateOfBirth = dateOfBirth;
            Gender = gender;
            Email = email;
            Phone = phone;
            Address = address;
            Program = program;
            EnrollmentYear = enrollmentYear;
        }

        public string FullName { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public string Gender { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string Program { get; set; } = string.Empty;

        public int EnrollmentYear { get; set; }
    }

    public sealed class StudentUpdateRequest
    {
        public StudentUpdateRequest()
        {
        }

        public StudentUpdateRequest(
            string fullName,
            DateTime dateOfBirth,
            string gender,
            string email,
            string phone,
            string address,
            string program,
            int enrollmentYear,
            StudentStatus status)
        {
            FullName = fullName;
            DateOfBirth = dateOfBirth;
            Gender = gender;
            Email = email;
            Phone = phone;
            Address = address;
            Program = program;
            EnrollmentYear = enrollmentYear;
            Status = status;
        }

        public string FullName { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public string Gender { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string Program { get; set; } = string.Empty;

        public int EnrollmentYear { get; set; }

        public StudentStatus Status { get; set; }
    }
}
