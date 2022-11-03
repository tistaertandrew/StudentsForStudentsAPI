namespace StudentsForStudentsAPI.Models.ViewModels
{
    public class SuccessViewModel
    {
        public bool Error { get; set; }
        public string Message { get; set; } = string.Empty;

        public SuccessViewModel(bool error, string message)
        {
            this.Error = error;
            this.Message = message;
        }
    }
}
