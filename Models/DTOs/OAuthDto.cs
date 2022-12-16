using System.ComponentModel.DataAnnotations;

namespace StudentsForStudentsAPI.Models.DTOs
{
    public class OAuthDto
    {
        [Required]
        public string Credentials { get; set; } = string.Empty;
    }
}
