namespace StudentsForStudentsAPI.Models.ViewModels
{
    public class UsernameViewModel
    {
        public string Username { get; set; } = string.Empty;

        public UsernameViewModel(string username)
        {
            this.Username = username;
        }
    }
}
