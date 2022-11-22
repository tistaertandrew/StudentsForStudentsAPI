namespace StudentsForStudentsAPI.Models
{
    public class File
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public User? Owner { get; set; }
    }
}