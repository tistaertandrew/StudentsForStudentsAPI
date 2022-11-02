using System.ComponentModel.DataAnnotations;

namespace StudentsForStudentsAPI.Models
{
    public class UserSignIn
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
