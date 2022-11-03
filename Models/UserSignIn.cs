using System.ComponentModel.DataAnnotations;

namespace StudentsForStudentsAPI.Models
{
    public class UserSignIn
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
