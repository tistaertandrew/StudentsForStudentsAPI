using System.ComponentModel.DataAnnotations;

namespace StudentsForStudentsAPI.Models
{
    public class UserSignUp
    {
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public int SectionId { get; set; } = int.MaxValue;
        [Required]
        public int CursusId { get; set; } = int.MaxValue;
        public string Password { get; set; } = string.Empty;
    }
}
