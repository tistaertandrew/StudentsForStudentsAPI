namespace StudentsForStudentsAPI.Models.ViewModels
{
    public class ErrorViewModel
    {
        public bool Error { get; set; }
        public string Message { get; set; }

        public ErrorViewModel(string message)
        {
            Error = true;
            Message = message;
        }
    }
}
