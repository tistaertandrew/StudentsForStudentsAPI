namespace StudentsForStudentsAPI.Models.DbModels
{
    public class File
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public Course Course { get; set; }
        public User? Owner { get; set; }
    }
}