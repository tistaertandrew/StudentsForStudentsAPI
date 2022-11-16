namespace StudentsForStudentsAPI.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public Cursus Cursus { get; set; }
    }
}
