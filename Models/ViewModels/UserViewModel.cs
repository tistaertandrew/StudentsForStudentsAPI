namespace StudentsForStudentsAPI.Models
{
    public class UserViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int CursusId { get; set; } = 0;
        public string Token { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } = false;
        public bool IsBanned { get; set; } = false;
        public UserViewModel(User user, bool isAdmin, string token)
        {
            this.Username = user.UserName;
            this.Email = user.Email;
            this.CursusId = user.Cursus.Id;
            this.IsBanned = user.IsBanned;
            this.Token = token;
            this.IsAdmin = isAdmin;
        }
    }
}
