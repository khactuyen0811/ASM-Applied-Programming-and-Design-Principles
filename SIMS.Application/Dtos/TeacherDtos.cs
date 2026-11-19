namespace SIMS.Application.Dtos
{
    public sealed class TeacherListItem
    {
        public TeacherListItem()
        {
        }

        public TeacherListItem(int id, string teacherCode, string fullName, string email, string department, int courseCount)
        {
            Id = id;
            TeacherCode = teacherCode;
            FullName = fullName;
            Email = email;
            Department = department;
            CourseCount = courseCount;
        }

        public int Id { get; set; }

        public string TeacherCode { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public int CourseCount { get; set; }
    }

    public sealed class TeacherDetails
    {
        public TeacherDetails()
        {
        }

        public TeacherDetails(int id, string teacherCode, string fullName, string email, string phone, string department, DateTime createdAt)
        {
            Id = id;
            TeacherCode = teacherCode;
            FullName = fullName;
            Email = email;
            Phone = phone;
            Department = department;
            CreatedAt = createdAt;
        }

        public int Id { get; set; }

        public string TeacherCode { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    public sealed class TeacherCreateRequest
    {
        public TeacherCreateRequest()
        {
        }

        public TeacherCreateRequest(string teacherCode, string fullName, string email, string phone, string department)
        {
            TeacherCode = teacherCode;
            FullName = fullName;
            Email = email;
            Phone = phone;
            Department = department;
        }

        public string TeacherCode { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;
    }

    public sealed class TeacherUpdateRequest
    {
        public TeacherUpdateRequest()
        {
        }

        public TeacherUpdateRequest(string teacherCode, string fullName, string email, string phone, string department)
        {
            TeacherCode = teacherCode;
            FullName = fullName;
            Email = email;
            Phone = phone;
            Department = department;
        }

        public string TeacherCode { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;
    }
}
