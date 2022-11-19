using System.Runtime.CompilerServices;

namespace StudentsForStudentsAPI.Models.ViewModels
{
    public class FileResponseViewModel<T>
    {

        public T? Content { get; set; }
        public bool IsError { get; set; } = false;
        public List<string> Errors { get; set; } = new List<string>();

        public FileResponseViewModel(bool isError, List<string> errors)
        {
            IsError = isError;
            Errors = errors;
            Content = default;
        }

        public FileResponseViewModel(T content)
        {
            IsError = false;
            Errors = new List<string>();
            Content = content;
        }

        public FileResponseViewModel(T content, bool isError, List<string> errors)
        {
            IsError = isError;
            Errors = errors;
            Content = content;
        }
    }
}
