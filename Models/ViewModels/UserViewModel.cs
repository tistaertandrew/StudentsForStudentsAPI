namespace StudentsForStudentsAPI.Models
{
    public class UserViewModel
    {   
        public int CursusId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public UserViewModel(User user, string token)
        {
            this.Username = user.UserName;
            this.Email = user.Email;
            this.Token = token;
            this.CursusId = user.Cursus.Id;
        }
    }
}
