namespace StudentsForStudentsAPI.Models.ViewModels
{
    public class SuccessViewModel
    {
        public bool Error { get; set; }
        public string Message { get; set; }

        public SuccessViewModel(string message)
        {
            Error = false;
            Message = message;
        }
    }
}
