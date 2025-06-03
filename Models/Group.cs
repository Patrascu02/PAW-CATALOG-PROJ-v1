namespace PAW_CATALOG_PROJ.Models
{
    public class Group
    {
        public int Id { get; set; }

        public string GroupNumber { get; set; } = null!;


        // A collection of students enrolled in this group
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }

}
