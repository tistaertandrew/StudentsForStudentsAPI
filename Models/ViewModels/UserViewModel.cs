namespace StudentsForStudentsAPI.Models
{
    public class UserViewModel
    {
        public bool Error { get; } = false;

        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public UserViewModel(User user, string token)
        {
            this.Username = user.UserName;
            this.Email = user.Email;
            this.Token = token;
        }
    }
}
