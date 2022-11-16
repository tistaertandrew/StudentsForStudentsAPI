namespace StudentsForStudentsAPI.Models
{
    public class UserViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int CursusId { get; set; } = 0;
        public string Token { get; set; } = string.Empty;
        public UserViewModel(User user, string token)
        {
            this.Username = user.UserName;
            this.Email = user.Email;
            this.CursusId = user.Cursus.Id;
            this.Token = token;
        }
    }
}
