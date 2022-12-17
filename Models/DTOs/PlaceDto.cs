using System.ComponentModel.DataAnnotations;

namespace StudentsForStudentsAPI.Models.DTOs
{
    public class PlaceDto
    {
        [Required]
        public string Street { get; set; } = string.Empty;
        [Required]
        public int PostalCode { get; set; } = 0;
        [Required]
        public string Number { get; set; } = string.Empty;
        [Required]
        public string Locality { get; set; } = string.Empty;
        
        public override string ToString() => $"{Street} {Number}, {PostalCode} {Locality}";
    }
}
