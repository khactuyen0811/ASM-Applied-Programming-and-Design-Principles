using System.ComponentModel.DataAnnotations;
using SIMS.Domain.Enums;

namespace SIMS.Domain.Entities
{
    public class AppUser
    {
        public int Id { get; set; }

        [Required, StringLength(60)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string PasswordSalt { get; set; } = string.Empty;

        [Required, StringLength(120)]
        public string DisplayName { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.Student;

        public int? StudentId { get; set; }

        public int? TeacherId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
