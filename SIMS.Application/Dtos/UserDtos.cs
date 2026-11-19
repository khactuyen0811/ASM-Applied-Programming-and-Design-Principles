using SIMS.Domain.Enums;

namespace SIMS.Application.Dtos
{
    public sealed class UserListItem
    {
        public UserListItem()
        {
        }

        public UserListItem(
            int id,
            string username,
            string displayName,
            UserRole role,
            bool isActive,
            int? studentId,
            int? teacherId,
            string linkedEmail,
            DateTime createdAt)
        {
            Id = id;
            Username = username;
            DisplayName = displayName;
            Role = role;
            IsActive = isActive;
            StudentId = studentId;
            TeacherId = teacherId;
            LinkedEmail = linkedEmail;
            CreatedAt = createdAt;
        }

        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public bool IsActive { get; set; }

        public int? StudentId { get; set; }

        public int? TeacherId { get; set; }

        public string LinkedEmail { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    public sealed class UserDetails
    {
        public UserDetails()
        {
        }

        public UserDetails(
            int id,
            string username,
            string displayName,
            UserRole role,
            bool isActive,
            int? studentId,
            int? teacherId,
            string linkedEmail,
            string linkedName,
            DateTime createdAt)
        {
            Id = id;
            Username = username;
            DisplayName = displayName;
            Role = role;
            IsActive = isActive;
            StudentId = studentId;
            TeacherId = teacherId;
            LinkedEmail = linkedEmail;
            LinkedName = linkedName;
            CreatedAt = createdAt;
        }

        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public bool IsActive { get; set; }

        public int? StudentId { get; set; }

        public int? TeacherId { get; set; }

        public string LinkedEmail { get; set; } = string.Empty;

        public string LinkedName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    public sealed class UpdateUserRoleRequest
    {
        public UpdateUserRoleRequest()
        {
        }

        public UpdateUserRoleRequest(int userId, UserRole role)
        {
            UserId = userId;
            Role = role;
        }

        public int UserId { get; set; }

        public UserRole Role { get; set; }
    }

    public sealed class ChangePasswordRequest
    {
        public ChangePasswordRequest()
        {
        }

        public ChangePasswordRequest(int userId, string currentPassword, string newPassword)
        {
            UserId = userId;
            CurrentPassword = currentPassword;
            NewPassword = newPassword;
        }

        public int UserId { get; set; }

        public string CurrentPassword { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;
    }

    public sealed class ResetPasswordResult
    {
        public ResetPasswordResult()
        {
        }

        public ResetPasswordResult(string username, string temporaryPassword)
        {
            Username = username;
            TemporaryPassword = temporaryPassword;
        }

        public string Username { get; set; } = string.Empty;

        public string TemporaryPassword { get; set; } = string.Empty;
    }

    public sealed class UserSummary
    {
        public UserSummary()
        {
        }

        public UserSummary(int total, int admins, int lecturers, int students, int active, int inactive)
        {
            Total = total;
            Admins = admins;
            Lecturers = lecturers;
            Students = students;
            Active = active;
            Inactive = inactive;
        }

        public int Total { get; set; }

        public int Admins { get; set; }

        public int Lecturers { get; set; }

        public int Students { get; set; }

        public int Active { get; set; }

        public int Inactive { get; set; }
    }
}
