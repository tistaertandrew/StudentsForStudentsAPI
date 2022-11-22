namespace StudentsForStudentsAPI.Models.ViewModels
{
    public class FileViewModel
    {
        public string Filename { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerId { get; set; }
        public int FileId { get; set; }
    }
}
