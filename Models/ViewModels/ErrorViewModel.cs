namespace StudentsForStudentsAPI.Models
{
    public class ErrorViewModel
    {
        public bool Error { get; set; }
        public string Message { get; set; } = string.Empty;

        public ErrorViewModel(bool error, string message)
        {
            this.Error = error;
            this.Message = message;
        }
    }
}
