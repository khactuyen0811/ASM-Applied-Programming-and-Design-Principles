using System.ComponentModel.DataAnnotations;

namespace SIMS.Domain.Entities
{
    public class Enrollment
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue)]
        public int StudentId { get; set; }

        [Range(1, int.MaxValue)]
        public int CourseId { get; set; }

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        [Range(0, 10)]
        public double? Grade { get; set; }
    }
}
