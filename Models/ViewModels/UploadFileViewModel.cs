namespace StudentsForStudentsAPI.Models.ViewModels
{
    public class UploadFileViewModel
    {
        public string Content { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;

        public UploadFileViewModel(string content, string fileName)
        {
            Content = content;
            FileName = fileName;
        }
    }
}
