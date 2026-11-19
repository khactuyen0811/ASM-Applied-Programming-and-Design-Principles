using SIMS.Domain.Enums;

namespace SIMS.Application.Dtos
{
    public sealed class LoginRequest
    {
        public LoginRequest()
        {
        }

        public LoginRequest(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }

    public sealed class AuthenticatedUser
    {
        public AuthenticatedUser()
        {
        }

        public AuthenticatedUser(int id, string username, string displayName, UserRole role, int? studentId, int? teacherId)
        {
            Id = id;
            Username = username;
            DisplayName = displayName;
            Role = role;
            StudentId = studentId;
            TeacherId = teacherId;
        }

        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public int? StudentId { get; set; }

        public int? TeacherId { get; set; }
    }
}
