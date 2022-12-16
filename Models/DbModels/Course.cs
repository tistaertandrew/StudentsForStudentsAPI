namespace StudentsForStudentsAPI.Models.DbModels
{
    public class Course
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public Cursus Cursus { get; set; }
    }
}
