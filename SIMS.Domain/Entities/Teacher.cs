using System.ComponentModel.DataAnnotations;

namespace SIMS.Domain.Entities
{
    public class Teacher
    {
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string TeacherCode { get; set; } = string.Empty;

        [Required, StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(160)]
        public string Email { get; set; } = string.Empty;

        [Phone, StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required, StringLength(120)]
        public string Department { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
