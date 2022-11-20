namespace StudentsForStudentsAPI.Models.ViewModels
{
    public class UploadFileViewModel
    {
        public string Content { get; set; } = string.Empty;
        public string Filename { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;

        public UploadFileViewModel(string content, string filename, string extension)
        {
            Content = content;
            Filename = filename;
            Extension = extension;
        }
    }
}