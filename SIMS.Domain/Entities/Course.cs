using System.ComponentModel.DataAnnotations;

namespace SIMS.Domain.Entities
{
    public class Course
    {
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string CourseCode { get; set; } = string.Empty;

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Range(1, 60)]
        public int Credits { get; set; } = 3;

        [Range(1, 1000)]
        public int Capacity { get; set; } = 30;

        public int? LecturerId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
