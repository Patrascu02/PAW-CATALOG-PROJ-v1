namespace PAW_CATALOG_PROJ.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string CourseName { get; set; } = null!;

        // a list of students enrolled in the course
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }

}
