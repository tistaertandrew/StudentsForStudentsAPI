namespace StudentsForStudentsAPI.Models.ViewModels
{
    public class UploadFileViewModel
    {
        public int CourseId { get; set; } = 0;
        public string Content { get; set; } = string.Empty;
        public string Filename { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;

        public UploadFileViewModel(int courseId, string content, string filename, string extension)
        {
            CourseId = courseId;
            Content = content;
            Filename = filename;
            Extension = extension;
        }
    }
}