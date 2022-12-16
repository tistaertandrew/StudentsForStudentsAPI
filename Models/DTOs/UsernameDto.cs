using System.ComponentModel.DataAnnotations;

namespace StudentsForStudentsAPI.Models.DTOs
{
    public class UsernameDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;
    }
}
