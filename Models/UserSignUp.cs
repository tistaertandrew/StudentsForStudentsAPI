using System.ComponentModel.DataAnnotations;

namespace StudentsForStudentsAPI.Models
{
    public class UserSignUp
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public int CursusId { get; set; } = int.MaxValue;
        public string Password { get; set; } = string.Empty;
    }
}
