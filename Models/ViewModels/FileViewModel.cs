namespace StudentsForStudentsAPI.Models.ViewModels
{
    public class FileViewModel
    {
        public string Filename { get; set; } = string.Empty;
        public Course Course { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public string OwnerId { get; set; }
        public int FileId { get; set; }
        public string Extension { get; set; } = string.Empty;
    }
}
