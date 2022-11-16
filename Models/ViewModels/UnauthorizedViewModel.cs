namespace StudentsForStudentsAPI.Models.ViewModels
{
    public class UnauthorizedViewModel
    {
        public bool Error { get; set; } = true;
        public bool Unauthorized { get; set; } = true;

        public UnauthorizedViewModel()
        {
            this.Error = true;
            this.Unauthorized = true;
        }
    }
}
