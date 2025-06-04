namespace PAW_CATALOG_PROJ.Models
{
    public class Grade
    {
        public int GradeId { get; set; }

        public int EnrollmentId { get; set; }
        public Enrollment? Enrollment { get; set; } = null!;

        public string Title { get; set; } = null!;

        public decimal GradeValue { get; set; }

        public DateTime DateRecorded { get; set; }
    }

}
