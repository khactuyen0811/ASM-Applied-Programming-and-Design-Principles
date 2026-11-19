using System.ComponentModel.DataAnnotations;

namespace SIMS.Domain.Entities
{
    public class Student
    {
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string StudentCode { get; set; } = string.Empty;

        [Required, StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; } = new DateTime(2005, 1, 1);

        [Required, StringLength(10)]
        public string Gender { get; set; } = "Other";

        [Required, EmailAddress, StringLength(160)]
        public string Email { get; set; } = string.Empty;

        [Phone, StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [Required, StringLength(120)]
        public string Program { get; set; } = string.Empty;

        [Range(2000, 2100)]
        public int EnrollmentYear { get; set; } = DateTime.UtcNow.Year;

        public SIMS.Domain.Enums.StudentStatus Status { get; set; } = SIMS.Domain.Enums.StudentStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
