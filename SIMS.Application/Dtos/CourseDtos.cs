namespace SIMS.Application.Dtos
{
    public sealed class CourseListItem
    {
        public CourseListItem()
        {
        }

        public CourseListItem(int id, string courseCode, string name, int credits, int capacity, int enrolledCount, string lecturerName)
        {
            Id = id;
            CourseCode = courseCode;
            Name = name;
            Credits = credits;
            Capacity = capacity;
            EnrolledCount = enrolledCount;
            LecturerName = lecturerName;
        }

        public int Id { get; set; }

        public string CourseCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int Credits { get; set; }

        public int Capacity { get; set; }

        public int EnrolledCount { get; set; }

        public string LecturerName { get; set; } = string.Empty;
    }

    public sealed class CourseDetails
    {
        public CourseDetails()
        {
        }

        public CourseDetails(
            int id,
            string courseCode,
            string name,
            string description,
            int credits,
            int capacity,
            int? lecturerId,
            string lecturerName,
            int enrolledCount)
        {
            Id = id;
            CourseCode = courseCode;
            Name = name;
            Description = description;
            Credits = credits;
            Capacity = capacity;
            LecturerId = lecturerId;
            LecturerName = lecturerName;
            EnrolledCount = enrolledCount;
        }

        public int Id { get; set; }

        public string CourseCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Credits { get; set; }

        public int Capacity { get; set; }

        public int? LecturerId { get; set; }

        public string LecturerName { get; set; } = string.Empty;

        public int EnrolledCount { get; set; }
    }

    public sealed class CourseCreateRequest
    {
        public CourseCreateRequest()
        {
        }

        public CourseCreateRequest(string courseCode, string name, string description, int credits, int capacity, int? lecturerId)
        {
            CourseCode = courseCode;
            Name = name;
            Description = description;
            Credits = credits;
            Capacity = capacity;
            LecturerId = lecturerId;
        }

        public string CourseCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Credits { get; set; }

        public int Capacity { get; set; }

        public int? LecturerId { get; set; }
    }

    public sealed class CourseUpdateRequest
    {
        public CourseUpdateRequest()
        {
        }

        public CourseUpdateRequest(string courseCode, string name, string description, int credits, int capacity, int? lecturerId)
        {
            CourseCode = courseCode;
            Name = name;
            Description = description;
            Credits = credits;
            Capacity = capacity;
            LecturerId = lecturerId;
        }

        public string CourseCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Credits { get; set; }

        public int Capacity { get; set; }

        public int? LecturerId { get; set; }
    }
}
