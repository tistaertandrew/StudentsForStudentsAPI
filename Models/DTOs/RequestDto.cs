using System.ComponentModel.DataAnnotations;

namespace StudentsForStudentsAPI.Models.DTOs
{
    public class RequestDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        public int PlaceId { get; set; } = 0;
        [Required]
        public int CourseId { get; set; } = 0;
    }
}
